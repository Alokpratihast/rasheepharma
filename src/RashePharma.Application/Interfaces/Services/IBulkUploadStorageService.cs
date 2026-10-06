namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType);

    Task<(Stream Stream, string ContentType)> DownloadAsync(
        string fileUrl);

    Task DeleteAsync(
        string fileUrl);
}