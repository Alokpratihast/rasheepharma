namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadStorageService
{
    Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);

    Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
        string fileName,
        CancellationToken cancellationToken = default);

    string GetStagedFileUrl(string blobName);

    Task<bool> VerifyStagedFileAsync(
        string blobName,
        long expectedFileSize,
        CancellationToken cancellationToken = default);

    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType);

    Task<(Stream Stream, string ContentType)> DownloadAsync(
        string fileUrl);

    Task DeleteAsync(
        string fileUrl);
}

/// <summary>
/// A short-lived, single-blob upload URL. It allows direct browser uploads
/// without exposing the storage account credentials to the client.
/// </summary>
public sealed record BulkUploadStorageTarget(
    string BlobName,
    string FileUrl,
    string UploadUrl);