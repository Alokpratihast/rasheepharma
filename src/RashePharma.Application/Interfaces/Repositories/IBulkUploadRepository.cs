
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

    Task<bool> TryRenewLeaseAsync(
        int jobId,
        string claimToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default);

    Task<bool> IsClaimOwnerAsync(
        int jobId,
        string claimToken,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateJobIfOwnedAsync(
        BulkUploadJob job,
        string claimToken,
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
