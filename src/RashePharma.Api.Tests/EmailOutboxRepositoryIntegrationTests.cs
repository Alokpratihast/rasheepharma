
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

using Xunit;

namespace RashePharma.Api.Tests;

public sealed class EmailOutboxRepositoryIntegrationTests
{
    private const string RequiredDatabase = "RashePharmaDb_IntegrationTests";

    private static string GetConnectionString()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before running integration tests.");

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var database = builder.ContainsKey("Database")
            ? builder["Database"]?.ToString()
            : builder["Initial Catalog"]?.ToString();

        if (!string.Equals(database, RequiredDatabase, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Integration tests must run only against {RequiredDatabase}. Current database: {database}");

        return connectionString;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static EmailJob CreateJob()
    {
        return new EmailJob
        {
            IdempotencyKey = $"outbox-test-{Guid.NewGuid():N}",
            Category = EmailCategory.Transactional,
            EmailType = "IntegrationTest",
            RecipientEmail = "integration-test@example.com",
            TemplateKey = "integration-test",
            TemplateVersion = 1,
            PayloadJson = "{}",
            CorrelationId = Guid.NewGuid().ToString("N")
        };
    }

    private static EmailOutboxMessage CreateOutbox(
        Guid jobId,
        string status,
        DateTime nowUtc,
        DateTime? nextAttemptAtUtc = null,
        DateTime? leaseUntilUtc = null)
    {
        return new EmailOutboxMessage
        {
            EmailJobId = jobId,
            EventType = "EmailRequested",
            SchemaVersion = 1,
            PayloadJson = "{}",
            Status = status,
            CreatedAtUtc = nowUtc,
            NextAttemptAtUtc = nextAttemptAtUtc,
            LeaseUntilUtc = leaseUntilUtc
        };
    }

    private static async Task<List<Guid>> SeedMessagesAsync(
        params EmailOutboxMessage[] messages)
    {
        await using var context = CreateContext();

        var jobs = messages
            .Select(x => x.EmailJobId)
            .Distinct()
            .Select(id => new EmailJob
            {
                Id = id,
                IdempotencyKey = $"outbox-test-{Guid.NewGuid():N}",
                Category = EmailCategory.Transactional,
                EmailType = "IntegrationTest",
                RecipientEmail = "integration-test@example.com",
                TemplateKey = "integration-test",
                TemplateVersion = 1,
                PayloadJson = "{}",
                CorrelationId = Guid.NewGuid().ToString("N")
            })
            .ToList();

        await context.EmailJobs.AddRangeAsync(jobs);
        await context.EmailOutboxMessages.AddRangeAsync(messages);
        await context.SaveChangesAsync();

        return messages.Select(x => x.Id).ToList();
    }

    private static async Task CleanupAsync(
        IEnumerable<Guid> outboxIds,
        IEnumerable<Guid> jobIds)
    {
        await using var context = CreateContext();

        var ids = outboxIds.ToArray();
        var jobs = jobIds.ToArray();

        await context.EmailOutboxMessages
            .Where(x => ids.Contains(x.Id))
            .ExecuteDeleteAsync();

        await context.EmailJobs
            .Where(x => jobs.Contains(x.Id))
            .ExecuteDeleteAsync();
    }

    [Fact]
    public async Task ClaimPendingAsync_ShouldClaimOnlyEligibleMessages()
    {
        var nowUtc = DateTime.UtcNow;
        var leaseDuration = TimeSpan.FromMinutes(2);

        var pendingJob = CreateJob();
        var futureRetryJob = CreateJob();
        var dueRetryJob = CreateJob();
        var activeLeaseJob = CreateJob();
        var expiredLeaseJob = CreateJob();

        var messages = new[]
        {
            CreateOutbox(
                pendingJob.Id,
                EmailOutboxStatus.Pending,
                nowUtc),

            CreateOutbox(
                futureRetryJob.Id,
                EmailOutboxStatus.RetryScheduled,
                nowUtc,
                nextAttemptAtUtc: nowUtc.AddMinutes(10)),

            CreateOutbox(
                dueRetryJob.Id,
                EmailOutboxStatus.RetryScheduled,
                nowUtc,
                nextAttemptAtUtc: nowUtc.AddMinutes(-1)),

            CreateOutbox(
                activeLeaseJob.Id,
                EmailOutboxStatus.Processing,
                nowUtc,
                leaseUntilUtc: nowUtc.AddMinutes(10)),

            CreateOutbox(
                expiredLeaseJob.Id,
                EmailOutboxStatus.Processing,
                nowUtc,
                leaseUntilUtc: nowUtc.AddMinutes(-1))
        };

        var outboxIds = messages.Select(x => x.Id).ToList();
        var jobIds = messages.Select(x => x.EmailJobId).ToList();

        try
        {
            await SeedMessagesAsync(messages);

            await using var context = CreateContext();
            var repository = new EmailOutboxRepository(context);

            var claimed = await repository.ClaimPendingAsync(
                batchSize: 10,
                nowUtc: nowUtc,
                leaseDuration: leaseDuration);

            var claimedIds = claimed.Select(x => x.Id).ToHashSet();

            Assert.Contains(messages[0].Id, claimedIds);
            Assert.Contains(messages[2].Id, claimedIds);

            Assert.DoesNotContain(messages[1].Id, claimedIds);
            Assert.DoesNotContain(messages[3].Id, claimedIds);

            // Expired Processing lease must be reclaimed.
            Assert.Contains(messages[4].Id, claimedIds);

            Assert.All(claimed, message =>
            {
                Assert.Equal(EmailOutboxStatus.Processing, message.Status);
                Assert.Equal(1, message.AttemptCount);
                Assert.NotNull(message.LeaseUntilUtc);
            });
        }
        finally
        {
            await CleanupAsync(outboxIds, jobIds);
        }
    }

    [Fact]
    public async Task ClaimPendingAsync_TwoConcurrentWorkers_ShouldNotClaimSameMessage()
    {
        var nowUtc = DateTime.UtcNow;
        const int messageCount = 8;

        var messages = Enumerable.Range(0, messageCount)
            .Select(_ =>
            {
                var job = CreateJob();

                return CreateOutbox(
                    job.Id,
                    EmailOutboxStatus.Pending,
                    nowUtc);
            })
            .ToArray();

        var outboxIds = messages.Select(x => x.Id).ToList();
        var jobIds = messages.Select(x => x.EmailJobId).ToList();

        try
        {
            await SeedMessagesAsync(messages);

            async Task<IReadOnlyList<EmailOutboxMessage>> RunWorkerAsync()
            {
                await using var context = CreateContext();
                var repository = new EmailOutboxRepository(context);

                return await repository.ClaimPendingAsync(
                    batchSize: messageCount,
                    nowUtc: nowUtc,
                    leaseDuration: TimeSpan.FromMinutes(2));
            }

            var results = await Task.WhenAll(
                RunWorkerAsync(),
                RunWorkerAsync());

            var firstWorkerIds = results[0].Select(x => x.Id).ToHashSet();
            var secondWorkerIds = results[1].Select(x => x.Id).ToHashSet();

            var allClaimedIds = results
                .SelectMany(x => x)
                .Select(x => x.Id)
                .ToList();

            // No message should be claimed by both workers.
            Assert.Empty(firstWorkerIds.Intersect(secondWorkerIds));

            // Every seeded message should be claimed exactly once.
            Assert.Equal(messageCount, allClaimedIds.Count);
            Assert.Equal(messageCount, allClaimedIds.Distinct().Count());
            Assert.Equal(
                messages.Select(x => x.Id).ToHashSet(),
                allClaimedIds.ToHashSet());
        }
        finally
        {
            await CleanupAsync(outboxIds, jobIds);
        }
    }
}
