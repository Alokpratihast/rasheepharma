using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class BulkUploadDispatchOutboxRepository : IBulkUploadDispatchOutboxRepository
{
    private readonly ApplicationDbContext _db;
    public BulkUploadDispatchOutboxRepository(ApplicationDbContext db) => _db = db;

    public async Task AddAsync(BulkUploadDispatchOutbox message, CancellationToken cancellationToken = default)
        => await _db.Set<BulkUploadDispatchOutbox>().AddAsync(message, cancellationToken);

    public async Task<IReadOnlyList<BulkUploadDispatchOutbox>> ClaimPendingAsync(
        int batchSize, DateTime nowUtc, TimeSpan leaseDuration, CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
        if (leaseDuration <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        var rows = await _db.Set<BulkUploadDispatchOutbox>().FromSqlInterpolated($@"
            SELECT TOP ({batchSize}) * FROM [BulkUploadDispatchOutbox] WITH (UPDLOCK, READPAST, ROWLOCK)
            WHERE (([Status] = {BulkUploadDispatchStatus.Pending})
                OR ([Status] = {BulkUploadDispatchStatus.RetryScheduled} AND [NextAttemptAtUtc] <= {nowUtc})
                OR ([Status] = {BulkUploadDispatchStatus.Processing} AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc] <= {nowUtc})))
            AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc] <= {nowUtc})
            ORDER BY [CreatedAtUtc], [Id]").ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            row.Status = BulkUploadDispatchStatus.Processing;
            row.LeaseUntilUtc = nowUtc.Add(leaseDuration);
            row.AttemptCount++;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return rows;
    }
}
