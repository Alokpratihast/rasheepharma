using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Bounded process-local signal. Multiple notifications coalesce into one
/// wake-up because the worker drains SQL in batches after waking.
/// </summary>
public sealed class EmailNotificationSignal : IEmailNotificationSignal
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
            // A wake-up is already pending; SQL contains the durable work.
        }
    }

    public Task<bool> WaitForWorkAsync(TimeSpan timeout, CancellationToken cancellationToken)
        => _signal.WaitAsync(timeout, cancellationToken);
}
