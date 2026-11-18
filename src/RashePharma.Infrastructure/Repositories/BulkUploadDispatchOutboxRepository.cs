using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class BulkUploadDispatchOutboxRepository : IBulkUploadDispatchOutboxRepository
{
    private readonly ApplicationDbContext _db;

    public BulkUploadDispatchOutboxRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        BulkUploadDispatchOutbox message,
        CancellationToken cancellationToken = default)
    {
        await _db.Set<BulkUploadDispatchOutbox>().AddAsync(message, cancellationToken);
    }

    /// <summary>
    /// Reopens published dispatch records when the corresponding job is still
    /// Pending or its Processing lease has expired. This repairs messages that
    /// were lost, dead-lettered, or acknowledged before the job reached a
    /// terminal state. Terminal jobs are deliberately not re-dispatched.
    /// </summary>
    public Task<int> RequeueExpiredProcessingJobsAsync(
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        return _db.Set<BulkUploadDispatchOutbox>()
            .Where(message =>
                message.Status == BulkUploadDispatchStatus.Published &&
                (
                    message.BulkUploadJob.Status == BulkUploadStatus.Pending ||
                    (
                        message.BulkUploadJob.Status == BulkUploadStatus.Processing &&
                        (
                            message.BulkUploadJob.LeaseExpiresAtUtc == null ||
                            message.BulkUploadJob.LeaseExpiresAtUtc <= nowUtc
                        )
                    )
                ))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(message => message.Status, BulkUploadDispatchStatus.Pending)
                    .SetProperty(message => message.PublishedAtUtc, (DateTime?)null)
                    .SetProperty(message => message.LeaseUntilUtc, (DateTime?)null)
                    .SetProperty(message => message.NextAttemptAtUtc, (DateTime?)null)
                    .SetProperty(
                        message => message.LastError,
                        "Requeued because the job is still Pending or its processing lease expired."),
                cancellationToken);
    }

    public async Task<IReadOnlyList<BulkUploadDispatchOutbox>> ClaimPendingAsync(
        int batchSize,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        var rows = await _db.Set<BulkUploadDispatchOutbox>()
            .FromSqlInterpolated($"""
                SELECT TOP ({batchSize}) *
                FROM [BulkUploadDispatchOutbox] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE
                    (
                        [Status] = {BulkUploadDispatchStatus.Pending}
                        OR
                        (
                            [Status] = {BulkUploadDispatchStatus.RetryScheduled}
                            AND [NextAttemptAtUtc] <= {nowUtc}
                        )
                        OR
                        (
                            [Status] = {BulkUploadDispatchStatus.Processing}
                            AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc] <= {nowUtc})
                        )
                    )
                    AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc] <= {nowUtc})
                ORDER BY [CreatedAtUtc], [Id]
                """)
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.Status = BulkUploadDispatchStatus.Processing;
            row.LeaseUntilUtc = nowUtc.Add(leaseDuration);
            row.AttemptCount++;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return rows;
    }
}
