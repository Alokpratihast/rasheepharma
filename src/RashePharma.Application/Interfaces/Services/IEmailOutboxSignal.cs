namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Wakes the API-owned outbox dispatcher after an EmailJob and its outbox
/// message have been committed. The SQL outbox remains durable.
/// </summary>
public interface IEmailOutboxSignal
{
    void NotifyMessageAvailable();

    Task<bool> WaitForMessageAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
