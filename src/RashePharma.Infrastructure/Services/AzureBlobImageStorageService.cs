using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class AzureBlobImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobImageStorageService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "Azure Storage connection string is required.",
                nameof(connectionString));
        }

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient("product-images");
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var blobClient = _containerClient.GetBlobClient(uniqueFileName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType ?? "application/octet-stream"
            }
        };

        await blobClient.UploadAsync(fileStream, uploadOptions);

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

        // Require the exact configured storage account host.
        if (!string.Equals(
                uri.Host,
                _containerClient.Uri.Host,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"The image URL host '{uri.Host}' does not match the configured storage account host '{_containerClient.Uri.Host}'.",
                nameof(imageUrl));
        }

        // Bind reads to the configured container; do not fetch arbitrary URLs.
        var containerPath = _containerClient.Uri.AbsolutePath.TrimEnd('/');
        var expectedPrefix = $"{containerPath}/";

        if (!uri.AbsolutePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The image URL is outside the configured image container.",
                nameof(imageUrl));
        }

        var encodedBlobName = uri.AbsolutePath[expectedPrefix.Length..];
        var blobName = Uri.UnescapeDataString(encodedBlobName);

        if (string.IsNullOrWhiteSpace(blobName) ||
            blobName.Contains('/') ||
            blobName.Contains('\\'))
        {
            throw new ArgumentException(
                "The image URL does not identify a valid image blob.",
                nameof(imageUrl));
        }

        // Use the configured container client and blob name; never download the supplied URL directly.
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync();

        var contentType = response.Value.Details.ContentType;

        return (
            response.Value.Content,
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType
        );
    }
}
