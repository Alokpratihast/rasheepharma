using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Process-local coalescing wake-up signal. A bounded semaphore prevents
/// repeated campaign submissions from building an unbounded in-memory queue.
/// The worker performs startup recovery and otherwise waits for campaign signals.
/// </summary>
public sealed class LeadOutreachJobSignal : ILeadOutreachJobSignal
{
    private readonly SemaphoreSlim _signal = new(0, 1);

    public void NotifyWorkAvailable()
    {
        try
        {
            _signal.Release();
        }
        catch (SemaphoreFullException)
        {
            // A pending signal already exists; notifications are coalesced.
        }
    }

    public Task<bool> WaitForWorkAsync(TimeSpan timeout, CancellationToken cancellationToken)
        => _signal.WaitAsync(timeout, cancellationToken);
}
