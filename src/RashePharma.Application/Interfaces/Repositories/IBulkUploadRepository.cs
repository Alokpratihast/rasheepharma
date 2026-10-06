using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IBulkUploadRepository
{
    Task<BulkUploadJob> CreateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default);

    Task<BulkUploadJob?> GetByIdAsync(
        int jobId,
        CancellationToken cancellationToken = default);

    Task<BulkUploadJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default);

    Task UpdateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default);

    Task AddErrorAsync(
        BulkUploadError error,
        CancellationToken cancellationToken = default);

    Task AddFileAsync(
        BulkUploadFile file,
        CancellationToken cancellationToken = default);
}