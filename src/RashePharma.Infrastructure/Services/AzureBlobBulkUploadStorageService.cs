using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class AzureBlobBulkUploadStorageService
    : IBulkUploadStorageService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobBulkUploadStorageService(
        string connectionString)
    {
        var blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(
                "bulk-upload-staging");
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

        var extension = Path.GetExtension(fileName);

        var uniqueFileName =
            $"{Guid.NewGuid():N}{extension}";

        var blobClient =
            _containerClient.GetBlobClient(uniqueFileName);

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
        string fileUrl)
    {
        var uri = new Uri(fileUrl);

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

    public async Task DeleteAsync(
        string fileUrl)
    {
        var uri = new Uri(fileUrl);

        var blobName =
            Path.GetFileName(uri.AbsolutePath);

        var blobClient =
            _containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }
}