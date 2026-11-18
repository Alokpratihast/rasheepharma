using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.BulkUpload;

namespace RashePharma.Api.Tests;

internal sealed class FakeBulkUploadDispatchOutboxRepository : IBulkUploadDispatchOutboxRepository
{
    public List<BulkUploadDispatchOutbox> Messages { get; } = new();

    public Task AddAsync(BulkUploadDispatchOutbox message, CancellationToken cancellationToken = default)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task<int> RequeueExpiredProcessingJobsAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
        => Task.FromResult(0);

    public Task<IReadOnlyList<BulkUploadDispatchOutbox>> ClaimPendingAsync(
        int batchSize,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<BulkUploadDispatchOutbox>>(Array.Empty<BulkUploadDispatchOutbox>());
}
