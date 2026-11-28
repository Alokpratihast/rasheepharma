using Xunit;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Infrastructure.Services;
using RashePharma.Infrastructure.Workers;

namespace RashePharma.Api.Tests;

public sealed class BulkUploadDispatchReliabilityTests
{
    [Fact]
    public void WorkerServiceRegistration_ValidatesAllConstructorDependencies()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=RasheePharmaTests;User Id=test;Password=not-used;TrustServerCertificate=True",
                ["Storage:RootPath"] = Path.Combine(Path.GetTempPath(), "rasheepharma-test-uploads"),
                ["RabbitMQ:Host"] = "localhost",
                ["RabbitMQ:Port"] = "5672",
                ["RabbitMQ:Username"] = "test",
                ["RabbitMQ:Password"] = "test",
                ["RabbitMQ:BulkUploadQueueName"] = "bulk-upload"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBulkUploadWorkerServices(configuration);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        Assert.NotNull(provider);
    }

    [Fact]
    public void ProgressUpdate_DoesNotMarkLeaseOwnershipFieldsAsModified()
    {
        var options = new DbContextOptionsBuilder<LeaseMetadataTestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new LeaseMetadataTestDbContext(options);
        var job = new BulkUploadJob
        {
            Id = 42,
            Status = RashePharma.Domain.Enums.BulkUploadStatus.Processing,
            ClaimToken = "active-claim",
            AttemptCount = 3,
            LeaseExpiresAtUtc = DateTime.UtcNow.AddMinutes(2),
            HeartbeatAtUtc = DateTime.UtcNow
        };

        context.BulkUploadJobs.Attach(job);
        context.Entry(job).State = EntityState.Modified;

        BulkUploadRepository.PreserveLeaseMetadata(context.Entry(job));

        Assert.False(context.Entry(job).Property(x => x.ClaimToken).IsModified);
        Assert.False(context.Entry(job).Property(x => x.AttemptCount).IsModified);
        Assert.False(context.Entry(job).Property(x => x.LeaseExpiresAtUtc).IsModified);
        Assert.False(context.Entry(job).Property(x => x.HeartbeatAtUtc).IsModified);
        Assert.True(context.Entry(job).Property(x => x.ProcessedRecords).IsModified);
    }

    [Fact]
    public void RetryWait_UsesScheduledRetryInsteadOfLongRecoveryInterval()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddSeconds(15);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            retryAt,
            now);

        Assert.Equal(TimeSpan.FromSeconds(15), wait);
    }

    [Fact]
    public void RetryWait_CapsLongRetryAtRecoveryInterval()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddMinutes(10);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            retryAt,
            now,
            TimeSpan.FromSeconds(60));

        Assert.Equal(TimeSpan.FromSeconds(60), wait);
    }

    [Fact]
    public void RetryWait_UsesRecoveryIntervalWhenNoRetryIsScheduled()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            null,
            now,
            TimeSpan.FromSeconds(60));

        Assert.Equal(TimeSpan.FromSeconds(60), wait);
    }

    [Fact]
    public void RetryWait_ReturnsImmediatelyWhenRetryIsAlreadyDue()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddSeconds(-1);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            retryAt,
            now);

        Assert.Equal(TimeSpan.Zero, wait);
    }

    private sealed class LeaseMetadataTestDbContext(
        DbContextOptions<LeaseMetadataTestDbContext> options) : DbContext(options)
    {
        public DbSet<BulkUploadJob> BulkUploadJobs => Set<BulkUploadJob>();
    }
}
