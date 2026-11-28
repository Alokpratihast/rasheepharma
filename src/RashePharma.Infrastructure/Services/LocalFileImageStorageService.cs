using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class LocalFileImageStorageService : IImageStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private readonly LocalFileStorageRoot _storage;
    private readonly string _prefix;

    public LocalFileImageStorageService(LocalFileStorageRoot storage, string prefix)
    {
        _storage = storage;
        _prefix = prefix;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName));
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Only JPEG, PNG, WebP, and GIF images are supported.", nameof(fileName));

        var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        return await _storage.WriteNewFileAsync(_prefix, storedName, fileStream);
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(string imageUrl)
    {
        var path = _storage.ResolveKey(imageUrl, _prefix);
        if (!File.Exists(path)) throw new FileNotFoundException("Stored image was not found.");

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return (stream, LocalFileStorageRoot.ContentTypeFor(path));
    }

    public Task DeleteAsync(string imageUrl)
    {
        if (!imageUrl.StartsWith($"local://{_prefix}/", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;
        var path = _storage.ResolveKey(imageUrl, _prefix);
        LocalFileStorageRoot.TryDelete(path);
        return Task.CompletedTask;
    }
}

public sealed class LocalFileBlogImageStorageService : LocalFileImageStorageService, IBlogImageStorageService
{
    public LocalFileBlogImageStorageService(LocalFileStorageRoot storage) : base(storage, "blogs") { }
}
