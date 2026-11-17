namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Wakes the local outreach producer after a campaign and its recipients
/// have been committed. SQL remains the durable source of truth.
/// </summary>
public interface ILeadOutreachJobSignal
{
    void NotifyWorkAvailable();

    Task<bool> WaitForWorkAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
