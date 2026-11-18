using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Data;
namespace RashePharma.Infrastructure.Repositories;
public sealed class BulkUploadDispatchOutboxRepository : IBulkUploadDispatchOutboxRepository
{
 private readonly ApplicationDbContext _db;
 public BulkUploadDispatchOutboxRepository(ApplicationDbContext db)=>_db=db;
 public async Task AddAsync(BulkUploadDispatchOutbox message,CancellationToken ct=default)=>await _db.Set<BulkUploadDispatchOutbox>().AddAsync(message,ct);
 public Task<int> RequeueExpiredProcessingJobsAsync(DateTime nowUtc,CancellationToken ct=default)
  => _db.Set<BulkUploadDispatchOutbox>().Where(x => x.Status == BulkUploadDispatchStatus.Published && x.BulkUploadJob.Status == RashePharma.Domain.Enums.BulkUploadStatus.Processing && x.BulkUploadJob.LeaseExpiresAtUtc != null && x.BulkUploadJob.LeaseExpiresAtUtc <= nowUtc)
    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, BulkUploadDispatchStatus.Pending).SetProperty(x => x.PublishedAtUtc, (DateTime?)null).SetProperty(x => x.LeaseUntilUtc, (DateTime?)null).SetProperty(x => x.NextAttemptAtUtc, (DateTime?)null).SetProperty(x => x.LastError, "Requeued after the processing lease expired."), ct);
 public async Task<IReadOnlyList<BulkUploadDispatchOutbox>> ClaimPendingAsync(int batchSize,DateTime nowUtc,TimeSpan leaseDuration,CancellationToken ct=default)
 {
  if(batchSize<=0) throw new ArgumentOutOfRangeException(nameof(batchSize));
  if(leaseDuration<=TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(leaseDuration));
  await using var tx=await _db.Database.BeginTransactionAsync(ct);
  var rows=await _db.Set<BulkUploadDispatchOutbox>().FromSqlInterpolated($@"
   SELECT TOP ({batchSize}) * FROM [BulkUploadDispatchOutbox] WITH (UPDLOCK, READPAST, ROWLOCK)
   WHERE (([Status]={BulkUploadDispatchStatus.Pending}) OR ([Status]={BulkUploadDispatchStatus.RetryScheduled} AND [NextAttemptAtUtc]<={nowUtc})
    OR ([Status]={BulkUploadDispatchStatus.Processing} AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc]<={nowUtc})))
   AND ([LeaseUntilUtc] IS NULL OR [LeaseUntilUtc]<={nowUtc}) ORDER BY [CreatedAtUtc],[Id]").ToListAsync(ct);
  foreach(var row in rows){row.Status=BulkUploadDispatchStatus.Processing;row.LeaseUntilUtc=nowUtc.Add(leaseDuration);row.AttemptCount++;}
  await _db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return rows;
 }
}