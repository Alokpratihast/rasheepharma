using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Process-local coalescing wake-up signal. The dispatcher also performs
/// startup and periodic SQL reconciliation to recover missed notifications.
/// </summary>
public sealed class BulkUploadDispatchSignal : IBulkUploadDispatchSignal
{
    private readonly SemaphoreSlim _signal = new(0, 1);

    public void NotifyMessageAvailable()
    {
        try
        {
            _signal.Release();
        }
        catch (SemaphoreFullException)
        {
            // One pending wake-up is sufficient to drain all eligible outbox rows.
        }
    }

    public Task<bool> WaitForMessageAsync(TimeSpan timeout, CancellationToken cancellationToken)
        => _signal.WaitAsync(timeout, cancellationToken);
}
