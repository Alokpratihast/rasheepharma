namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Wakes the local bulk-upload processor after a job and its file manifest
/// have been committed. The database remains the durable source of truth.
/// </summary>
public interface IBulkUploadJobSignal
{
    void NotifyJobAvailable();
}
