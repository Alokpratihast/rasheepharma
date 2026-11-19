using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Process-local coalescing wake-up signal. Startup reconciliation and
/// scheduled retry times recover durable outbox messages without idle polling.
/// </summary>
public sealed class EmailOutboxSignal : IEmailOutboxSignal
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
            // One pending wake-up is enough to drain all eligible outbox rows.
        }
    }

    public Task<bool> WaitForMessageAsync(TimeSpan timeout, CancellationToken cancellationToken)
        => _signal.WaitAsync(timeout, cancellationToken);
}
