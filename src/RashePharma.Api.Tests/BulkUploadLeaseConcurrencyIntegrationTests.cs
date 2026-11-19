using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using Xunit;

namespace RashePharma.Api.Tests;

[Collection("Database integration")]
public sealed class BulkUploadLeaseConcurrencyIntegrationTests
{
    private const string RequiredDatabase = "RashePharmaDb_IntegrationTests";

    private static string GetConnectionString()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection before running SQL Server integration tests.");
        }

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var database = builder.ContainsKey("Database")
            ? builder["Database"]?.ToString()
            : builder.ContainsKey("Initial Catalog")
                ? builder["Initial Catalog"]?.ToString()
                : null;

        if (!string.Equals(database, RequiredDatabase, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Bulk upload integration tests may run only against {RequiredDatabase}. Current database: {database}");
        }

        return connectionString;
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(GetConnectionString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task HeartbeatRenewal_ThenStaleProgressUpdate_PreservesLease_AndBlocksSecondWorkerClaim()
    {
        var claimToken = Guid.NewGuid().ToString("N");
        var job = new BulkUploadJob
        {
            FileName = $"lease-concurrency-{Guid.NewGuid():N}.xlsx",
            Status = BulkUploadStatus.Processing,
            TotalRecords = 20,
            ProcessedRecords = 2,
            SuccessCount = 2,
            ErrorCount = 0,
            AttemptCount = 1,
            ClaimToken = claimToken,
            HeartbeatAtUtc = DateTime.UtcNow,
            LeaseExpiresAtUtc = DateTime.UtcNow.AddMinutes(2),
            CreatedAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow
        };

        var jobId = 0;

        try
        {
            await using (var seedContext = CreateContext())
            {
                seedContext.BulkUploadJobs.Add(job);
                await seedContext.SaveChangesAsync();
                jobId = job.Id;
            }

            // Worker A's processing scope loads a snapshot before the heartbeat.
            await using var processingContext = CreateContext();
            var staleJob = await processingContext.BulkUploadJobs
                .SingleAsync(x => x.Id == jobId);

            var staleLeaseExpiry = staleJob.LeaseExpiresAtUtc;
            var staleHeartbeat = staleJob.HeartbeatAtUtc;

            // The heartbeat scope renews the lease using a separate DbContext.
            await using (var heartbeatContext = CreateContext())
            {
                var heartbeatRepository = new BulkUploadRepository(heartbeatContext);

                var renewed = await heartbeatRepository.TryRenewLeaseAsync(
                    jobId,
                    claimToken,
                    TimeSpan.FromMinutes(2));

                Assert.True(renewed);
            }

            DateTime? renewedLeaseExpiry;
            DateTime? renewedHeartbeat;

            await using (var verificationContext = CreateContext())
            {
                var renewedJob = await verificationContext.BulkUploadJobs
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == jobId);

                renewedLeaseExpiry = renewedJob.LeaseExpiresAtUtc;
                renewedHeartbeat = renewedJob.HeartbeatAtUtc;

                Assert.NotNull(renewedLeaseExpiry);
                Assert.NotNull(renewedHeartbeat);
                Assert.True(renewedLeaseExpiry > staleLeaseExpiry);
                Assert.True(renewedHeartbeat > staleHeartbeat);
            }

            // The old processing snapshot now saves progress after the heartbeat.
            staleJob.ProcessedRecords = 10;
            staleJob.SuccessCount = 10;

            var processingRepository = new BulkUploadRepository(processingContext);
            var progressSaved = await processingRepository.UpdateJobIfOwnedAsync(
                staleJob,
                claimToken);

            Assert.True(progressSaved);

            // Verify the persisted SQL row, not the processing DbContext's tracked entity.
            await using (var finalContext = CreateContext())
            {
                var persistedJob = await finalContext.BulkUploadJobs
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == jobId);

                Assert.Equal(10, persistedJob.ProcessedRecords);
                Assert.Equal(10, persistedJob.SuccessCount);
                Assert.Equal(claimToken, persistedJob.ClaimToken);
                Assert.Equal(1, persistedJob.AttemptCount);
                Assert.Equal(renewedLeaseExpiry, persistedJob.LeaseExpiresAtUtc);
                Assert.Equal(renewedHeartbeat, persistedJob.HeartbeatAtUtc);
            }

            // Worker B must not claim a job while Worker A's renewed lease is active.
            await using (var secondWorkerContext = CreateContext())
            {
                var secondWorkerRepository = new BulkUploadRepository(secondWorkerContext);

                var secondClaim = await secondWorkerRepository.ClaimJobAsync(jobId);

                Assert.Null(secondClaim);
            }
        }
        finally
        {
            if (jobId > 0)
            {
                await using var cleanupContext = CreateContext();
                await cleanupContext.BulkUploadJobs
                    .Where(x => x.Id == jobId)
                    .ExecuteDeleteAsync();
            }
        }
    }
}
