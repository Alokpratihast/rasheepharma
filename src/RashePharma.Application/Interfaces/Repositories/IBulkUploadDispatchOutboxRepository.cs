using RashePharma.Domain.Entities.BulkUpload;
namespace RashePharma.Application.Interfaces.Repositories;
public interface IBulkUploadDispatchOutboxRepository {
 Task AddAsync(BulkUploadDispatchOutbox message,CancellationToken cancellationToken=default);
 Task<int> RequeueExpiredProcessingJobsAsync(DateTime nowUtc,CancellationToken cancellationToken=default);
 Task<IReadOnlyList<BulkUploadDispatchOutbox>> ClaimPendingAsync(int batchSize,DateTime nowUtc,TimeSpan leaseDuration,CancellationToken cancellationToken=default);
}