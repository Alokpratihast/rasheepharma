namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Process-local coalescing wake-up signal. SQL remains the durable source
/// of truth; the worker periodically reconciles to recover missed signals
/// and notifications whose retry delay has elapsed.
/// </summary>
public interface IEmailNotificationSignal
{
    void NotifyWorkAvailable();

    Task<bool> WaitForWorkAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
