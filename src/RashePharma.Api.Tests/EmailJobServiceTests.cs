
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class EmailJobServiceTests
{
    // TEST 1: Valid job creation
    [Fact]
    public async Task CreateJobAsync_ValidJob_CreatesJobAndOutboxAndCommits()
    {
        var (service, jobs, outbox, unitOfWork) = CreateService();

        var job = new EmailJob
        {
            IdempotencyKey = "lead-100-campaign-1",
            RecipientEmail = "customer@example.com"
        };

        var result = await service.CreateJobAsync(job);

        Assert.Equal(job.Id, result.Id);
        Assert.Single(jobs.Items);
        Assert.Single(outbox.Items);
        Assert.Equal(job.Id, outbox.Items[0].EmailJobId);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Equal(1, unitOfWork.Transaction.CommitCount);
        Assert.Equal(0, unitOfWork.Transaction.RollbackCount);
    }

    // TEST 2: Existing idempotency key
    [Fact]
    public async Task CreateJobAsync_ExistingIdempotencyKey_ReturnsExistingJob()
    {
        var (service, jobs, outbox, unitOfWork) = CreateService();

        var existingJob = new EmailJob
        {
            IdempotencyKey = "lead-100-campaign-2",
            RecipientEmail = "customer@example.com"
        };

        jobs.Items.Add(existingJob);

        var newRequest = new EmailJob
        {
            IdempotencyKey = existingJob.IdempotencyKey,
            RecipientEmail = "another@example.com"
        };

        var result = await service.CreateJobAsync(newRequest);

        Assert.Equal(existingJob.Id, result.Id);
        Assert.Single(jobs.Items);
        Assert.Empty(outbox.Items);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
        Assert.Equal(0, unitOfWork.Transaction.CommitCount);
    }

    // TEST 3: Empty idempotency key
    [Fact]
    public async Task CreateJobAsync_EmptyIdempotencyKey_ThrowsArgumentException()
    {
        var (service, _, _, _) = CreateService();

        var job = new EmailJob
        {
            IdempotencyKey = "",
            RecipientEmail = "customer@example.com"
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateJobAsync(job));
    }

    // TEST 4: Empty recipient email
    [Fact]
    public async Task CreateJobAsync_EmptyRecipient_ThrowsArgumentException()
    {
        var (service, _, _, _) = CreateService();

        var job = new EmailJob
        {
            IdempotencyKey = "lead-100-campaign-3",
            RecipientEmail = " "
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateJobAsync(job));
    }

    // TEST 5: SaveChanges failure
    [Fact]
    public async Task CreateJobAsync_SaveChangesFails_RollsBackTransaction()
    {
        var (service, jobs, outbox, unitOfWork) = CreateService();

        unitOfWork.ShouldFailOnSaveChanges = true;

        var job = new EmailJob
        {
            IdempotencyKey = "lead-rollback-1001",
            RecipientEmail = "customer@example.com"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateJobAsync(job));

        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Equal(0, unitOfWork.Transaction.CommitCount);
        Assert.Equal(1, unitOfWork.Transaction.RollbackCount);

        Assert.Single(jobs.Items);
        Assert.Single(outbox.Items);
    }

    private static (
        EmailJobService Service,
        FakeEmailJobRepository Jobs,
        FakeEmailOutboxRepository Outbox,
        FakeUnitOfWork UnitOfWork) CreateService()
    {
        var jobs = new FakeEmailJobRepository();
        var outbox = new FakeEmailOutboxRepository();
        var unitOfWork = new FakeUnitOfWork();

        var serviceProvider = new ServiceCollection()
            .AddScoped<IEmailJobRepository>(_ => jobs)
            .AddScoped<IEmailOutboxRepository>(_ => outbox)
            .BuildServiceProvider();

        var scopeFactory =
            serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var service = new EmailJobService(
            jobs,
            outbox,
            unitOfWork,
            scopeFactory);

        return (service, jobs, outbox, unitOfWork);
    }

    private sealed class FakeEmailJobRepository : IEmailJobRepository
    {
        public List<EmailJob> Items { get; } = [];

        public Task<EmailJob?> GetByIdAsync(Guid id) =>
            Task.FromResult(
                Items.FirstOrDefault(x => x.Id == id));

        public Task<EmailJob?> GetByIdempotencyKeyAsync(
            string idempotencyKey) =>
            Task.FromResult(
                Items.FirstOrDefault(
                    x => x.IdempotencyKey == idempotencyKey));

        public Task AddAsync(EmailJob emailJob)
        {
            Items.Add(emailJob);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(EmailJob emailJob) =>
            Task.CompletedTask;
    }

    private sealed class FakeEmailOutboxRepository : IEmailOutboxRepository
    {
        public List<EmailOutboxMessage> Items { get; } = [];

        public Task<EmailOutboxMessage?> GetByIdAsync(Guid id) =>
            Task.FromResult(
                Items.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(EmailOutboxMessage outboxMessage)
        {
            Items.Add(outboxMessage);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(EmailOutboxMessage outboxMessage) =>
            Task.CompletedTask;

        // Added to satisfy IEmailOutboxRepository interface.
        public Task<IReadOnlyList<EmailOutboxMessage>> ClaimPendingAsync(
            int batchSize,
            DateTime nowUtc,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<EmailOutboxMessage>>(
                Array.Empty<EmailOutboxMessage>());
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public bool ShouldFailOnSaveChanges { get; set; }

        public FakeTransaction Transaction { get; } = new();

        public Task<int> SaveChangesAsync()
        {
            SaveChangesCount++;

            if (ShouldFailOnSaveChanges)
            {
                throw new InvalidOperationException(
                    "Simulated database save failure.");
            }

            return Task.FromResult(1);
        }

        public Task<ITransaction> BeginTransactionAsync() =>
            Task.FromResult<ITransaction>(Transaction);
    }

    private sealed class FakeTransaction : ITransaction
    {
        public int CommitCount { get; private set; }

        public int RollbackCount { get; private set; }

        public bool ShouldFailOnCommit { get; set; }

        public Task CommitAsync()
        {
            if (ShouldFailOnCommit)
            {
                throw new InvalidOperationException(
                    "Simulated transaction commit failure.");
            }

            CommitCount++;
            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            RollbackCount++;
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }

    // TEST 6: Commit failure
    [Fact]
    public async Task CreateJobAsync_CommitFails_RollsBackTransaction()
    {
        var (service, jobs, outbox, unitOfWork) = CreateService();

        unitOfWork.Transaction.ShouldFailOnCommit = true;

        var job = new EmailJob
        {
            IdempotencyKey = "lead-commit-failure-1001",
            RecipientEmail = "customer@example.com"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateJobAsync(job));

        Assert.Single(jobs.Items);
        Assert.Single(outbox.Items);

        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Equal(0, unitOfWork.Transaction.CommitCount);
        Assert.Equal(1, unitOfWork.Transaction.RollbackCount);
    }
}
