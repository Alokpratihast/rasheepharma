namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Wakes the API-owned bulk-upload outbox dispatcher after the job and
/// dispatch outbox record have committed. SQL remains the durable source of truth.
/// </summary>
public interface IBulkUploadDispatchSignal
{
    void NotifyMessageAvailable();

    Task<bool> WaitForMessageAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
