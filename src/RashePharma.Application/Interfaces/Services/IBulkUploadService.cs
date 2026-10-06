namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadService
{
    Task<int> CreateJobAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    Task<int> CreateJobAsync(
        Stream excelStream,
        string excelFileName,
        IReadOnlyCollection<BulkUploadFileInput> files,
        CancellationToken cancellationToken = default);

    Task ProcessAsync(
        int jobId,
        CancellationToken cancellationToken = default);

    Task<object?> GetStatusAsync(
        int jobId,
        CancellationToken cancellationToken = default);
}

public record BulkUploadFileInput(
    Stream Stream,
    string FileName,
    string? ContentType,
    long FileSize);