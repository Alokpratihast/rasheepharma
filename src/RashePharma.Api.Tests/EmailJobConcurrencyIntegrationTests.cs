
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class EmailJobConcurrencyIntegrationTests
{
    private const string RequiredDatabase =
        "RashePharmaDb_IntegrationTests";

    [Fact]
    public async Task CreateJobAsync_ConcurrentSameKey_CreatesOnlyOneJobAndOutbox()
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        Assert.False(
            string.IsNullOrWhiteSpace(connectionString),
            "Set ConnectionStrings__DefaultConnection before running this test.");

        var connectionBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString!
        };

        var databaseName =
            connectionBuilder.ContainsKey("Initial Catalog")
                ? Convert.ToString(connectionBuilder["Initial Catalog"])
                : connectionBuilder.ContainsKey("Database")
                    ? Convert.ToString(connectionBuilder["Database"])
                    : null;

        Assert.Equal(RequiredDatabase, databaseName);

        var idempotencyKey =
            $"integration-concurrency-{Guid.NewGuid():N}";

        var readBarrier = new InitialReadBarrier();

        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(
            options => options.UseSqlServer(connectionString));

        services.AddScoped<EmailJobRepository>();

        services.AddScoped<IEmailJobRepository>(provider =>
            new SynchronizingEmailJobRepository(
                provider.GetRequiredService<EmailJobRepository>(),
                readBarrier));

        services.AddScoped<
            IEmailOutboxRepository,
            EmailOutboxRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IEmailJobService, EmailJobService>();

        using var serviceProvider = services.BuildServiceProvider();

        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var service1 =
            scope1.ServiceProvider.GetRequiredService<IEmailJobService>();

        var service2 =
            scope2.ServiceProvider.GetRequiredService<IEmailJobService>();

        var job1 = CreateJob(idempotencyKey);
        var job2 = CreateJob(idempotencyKey);

        try
        {
            var task1 = service1.CreateJobAsync(job1);
            var task2 = service2.CreateJobAsync(job2);

            var results = await Task.WhenAll(task1, task2)
                .WaitAsync(TimeSpan.FromSeconds(30));

            Assert.Equal(results[0].Id, results[1].Id);

            using var verificationScope =
                serviceProvider.CreateScope();

            var db = verificationScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var jobs = await db.EmailJobs
                .Where(x => x.IdempotencyKey == idempotencyKey)
                .ToListAsync();

            Assert.Single(jobs);

            var outboxMessages = await db.EmailOutboxMessages
                .Where(x =>
                    x.EmailJobId == jobs[0].Id &&
                    x.EventType == "EmailRequested")
                .ToListAsync();

            Assert.Single(outboxMessages);

            Assert.Equal("Pending", outboxMessages[0].Status);
        }
        finally
        {
            // Remove only the records created by this test.
            using var cleanupScope = serviceProvider.CreateScope();

            var db = cleanupScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var matchingJobIds = db.EmailJobs
                .Where(x => x.IdempotencyKey == idempotencyKey)
                .Select(x => x.Id);

            await db.EmailOutboxMessages
                .Where(x => matchingJobIds.Contains(x.EmailJobId))
                .ExecuteDeleteAsync();

            await db.EmailJobs
                .Where(x => x.IdempotencyKey == idempotencyKey)
                .ExecuteDeleteAsync();
        }
    }

    private static EmailJob CreateJob(string idempotencyKey)
    {
        return new EmailJob
        {
            IdempotencyKey = idempotencyKey,
            Category = EmailCategory.Transactional,
            EmailType = "LeadOutreach",
            RecipientEmail = "integration-test@example.com",
            TemplateKey = "lead-outreach",
            TemplateVersion = 1,
            PayloadJson = "{}",
            CorrelationId = Guid.NewGuid().ToString("N")
        };
    }

    private sealed class InitialReadBarrier
    {
        private readonly TaskCompletionSource<bool> _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private int _initialReadCount;

        public async Task WaitForBothInitialReadsAsync()
        {
            var count = Interlocked.Increment(
                ref _initialReadCount);

            if (count == 2)
            {
                _completion.TrySetResult(true);
            }

            if (count <= 2)
            {
                await _completion.Task.WaitAsync(
                    TimeSpan.FromSeconds(20));
            }
        }
    }

    private sealed class SynchronizingEmailJobRepository
        : IEmailJobRepository
    {
        private readonly IEmailJobRepository _inner;
        private readonly InitialReadBarrier _barrier;

        public SynchronizingEmailJobRepository(
            IEmailJobRepository inner,
            InitialReadBarrier barrier)
        {
            _inner = inner;
            _barrier = barrier;
        }

        public async Task<EmailJob?> GetByIdempotencyKeyAsync(
            string idempotencyKey)
        {
            var existingJob =
                await _inner.GetByIdempotencyKeyAsync(idempotencyKey);

            await _barrier.WaitForBothInitialReadsAsync();

            return existingJob;
        }

        public Task<EmailJob?> GetByIdAsync(Guid id) =>
            _inner.GetByIdAsync(id);

        public Task AddAsync(EmailJob emailJob) =>
            _inner.AddAsync(emailJob);

        public Task UpdateAsync(EmailJob emailJob) =>
            _inner.UpdateAsync(emailJob);
    }
}
