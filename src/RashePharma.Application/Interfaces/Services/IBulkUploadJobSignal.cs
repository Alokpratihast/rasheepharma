namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Wakes the local bulk-upload processor after a complete job manifest has
/// been committed. The database remains the durable source of truth.
/// </summary>
public interface IBulkUploadJobSignal
{
    void NotifyJobAvailable();

    Task<bool> WaitForJobAsync(TimeSpan timeout, CancellationToken cancellationToken);
}
