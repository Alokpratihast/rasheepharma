namespace RashePharma.Application.Interfaces.Services;

public interface IImageStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType);

    Task<(Stream Stream, string ContentType)> DownloadAsync(
        string imageUrl);
}