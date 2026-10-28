using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>Stores new blog media separately and reads legacy product image URLs for existing posts.</summary>
public sealed class AzureBlobBlogImageStorageService : IBlogImageStorageService
{
    private const string BlogContainerName = "blog-images";
    private const string LegacyContainerName = "product-images";
    private readonly BlobContainerClient _blogContainer;
    private readonly BlobContainerClient _legacyProductContainer;

    public AzureBlobBlogImageStorageService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Azure Storage connection string is required.",
                nameof(connectionString));
        }

        var blobServiceClient = new BlobServiceClient(connectionString);
        _blogContainer = blobServiceClient.GetBlobContainerClient(BlogContainerName);
        _legacyProductContainer = blobServiceClient.GetBlobContainerClient(LegacyContainerName);
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType)
    {
        await _blogContainer.CreateIfNotExistsAsync(PublicAccessType.None);

        var extension = Path.GetExtension(fileName);
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blobClient = _blogContainer.GetBlobClient(blobName);
        await blobClient.UploadAsync(fileStream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType ?? "application/octet-stream"
            }
        });

        return blobClient.Uri.ToString();
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(string imageUrl)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             !(uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)))
        {
            throw new ArgumentException(
                "The image URL must be an absolute HTTPS Azure Blob URL (or loopback HTTP for local development).",
                nameof(imageUrl));
        }

        var blobClient = GetAllowedBlobClient(uri, _blogContainer)
            ?? GetAllowedBlobClient(uri, _legacyProductContainer)
            ?? throw new ArgumentException(
                "The image URL is outside the configured blog image containers.",
                nameof(imageUrl));

        // Resolve the URL only against the configured blog or legacy product container clients.
        var response = await blobClient.DownloadStreamingAsync();
        var contentType = response.Value.Details.ContentType;
        return (
            response.Value.Content,
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);
    }

    private static BlobClient? GetAllowedBlobClient(
        Uri uri,
        BlobContainerClient container)
    {
        if (!string.Equals(uri.Host, container.Uri.Host, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var expectedPrefix = $"{container.Uri.AbsolutePath.TrimEnd('/')}/";
        if (!uri.AbsolutePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var blobName = Uri.UnescapeDataString(uri.AbsolutePath[expectedPrefix.Length..]);
        if (string.IsNullOrWhiteSpace(blobName) ||
            blobName.Contains('/') ||
            blobName.Contains('\\'))
        {
            throw new ArgumentException(
                "The image URL does not identify a valid image blob.",
                nameof(uri));
        }

        return container.GetBlobClient(blobName);
    }
}
