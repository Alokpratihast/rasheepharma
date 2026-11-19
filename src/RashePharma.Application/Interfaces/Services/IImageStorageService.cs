namespace RashePharma.Application.Interfaces.Services;

public interface IImageStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType);

    Task<(Stream Stream, string ContentType)> DownloadAsync(
        string imageUrl);

    /// <summary>Deletes an image only when its URL belongs to the configured image container.</summary>
    Task DeleteAsync(string imageUrl);
}