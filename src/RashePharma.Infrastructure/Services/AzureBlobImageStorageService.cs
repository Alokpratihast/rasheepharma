using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class AzureBlobImageStorageService : IImageStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobImageStorageService(string connectionString)
    {
        var blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(
                "product-images");
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

        var extension =
            Path.GetExtension(fileName);

        var uniqueFileName =
            $"{Guid.NewGuid():N}{extension}";

        var blobClient =
            _containerClient.GetBlobClient(
                uniqueFileName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType =
                    contentType ??
                    "application/octet-stream"
            }
        };

        await blobClient.UploadAsync(
            fileStream,
            uploadOptions);

        return blobClient.Uri.ToString();
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(
        string imageUrl)
    {
        var uri = new Uri(imageUrl);

        var blobName =
            Path.GetFileName(uri.AbsolutePath);

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        var response =
            await blobClient.DownloadStreamingAsync();

        var contentType =
            response.Value.Details.ContentType;

        return (
            response.Value.Content,
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType
        );
    }
}