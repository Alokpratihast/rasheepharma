namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadStorageService
{
    Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default);

    Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
        string fileName,
        long expectedFileSize,
        CancellationToken cancellationToken = default);

    Task WriteUploadChunkAsync(
        string blobName,
        int chunkIndex,
        long expectedFileSize,
        Stream chunk,
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

    Task CleanupExpiredUploadsAsync(
        TimeSpan maxAge,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// A generated staging file name and authenticated API upload route.
/// </summary>
public sealed record BulkUploadStorageTarget(
    string BlobName,
    string FileUrl,
    string UploadUrl);
