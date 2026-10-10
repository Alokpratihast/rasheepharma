using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class AzureBlobBulkUploadStorageService
    : IBulkUploadStorageService
{
    private const string ContainerName = "bulk-upload-staging";
    private readonly BlobContainerClient _containerClient;

    public AzureBlobBulkUploadStorageService(
        string connectionString)
    {
        var blobServiceClient =
            new BlobServiceClient(connectionString);

        _containerClient =
            blobServiceClient.GetBlobContainerClient(ContainerName);
    }

    /// <summary>
    /// Creates the private staging container once before issuing targets.
    /// </summary>
    public async Task EnsureContainerExistsAsync(
        CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a random staging blob and returns a one-hour SAS that grants
    /// only create/write access to that blob. Large bytes go directly from the
    /// browser to Blob Storage instead of passing through the API.
    /// </summary>
    public Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
        string fileName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var extension = Path.GetExtension(Path.GetFileName(fileName));
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "Azure Blob upload URLs cannot be generated. Configure Azure Storage with credentials that support SAS generation.");
        }

        var sas = new BlobSasBuilder
        {
            BlobContainerName = ContainerName,
            BlobName = blobName,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            Protocol = SasProtocol.Https
        };
        sas.SetPermissions(
            BlobSasPermissions.Create |
            BlobSasPermissions.Write);

        return Task.FromResult(new BulkUploadStorageTarget(
            blobName,
            blobClient.Uri.ToString(),
            blobClient.GenerateSasUri(sas).ToString()));
    }
    public string GetStagedFileUrl(string blobName) =>
        _containerClient.GetBlobClient(blobName).Uri.ToString();

    public async Task<bool> VerifyStagedFileAsync(
        string blobName,
        long expectedFileSize,
        CancellationToken cancellationToken = default)
    {
        if (!IsGeneratedBlobName(blobName) || expectedFileSize <= 0)
        {
            return false;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);

        try
        {
            var properties = await blobClient.GetPropertiesAsync(
                cancellationToken: cancellationToken);

            if (properties.Value.BlobType != BlobType.Block ||
                properties.Value.ContentLength != expectedFileSize)
            {
                return false;
            }

            // File names and browser MIME types are user-controlled. Read only
            // a small signature prefix to reject obvious type spoofing without
            // buffering the uploaded file into API memory.
            var download = await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);
            await using var stream = download.Value.Content;
            var header = new byte[12];
            var bytesRead = 0;
            while (bytesRead < header.Length)
            {
                var read = await stream.ReadAsync(
                    header.AsMemory(bytesRead),
                    cancellationToken);
                if (read == 0) break;
                bytesRead += read;
            }

            return HasAllowedSignature(blobName, header.AsSpan(0, bytesRead));
        }
        catch (RequestFailedException exception)
            when (exception.Status == 404)
        {
            return false;
        }
    }


    private static bool HasAllowedSignature(string blobName, ReadOnlySpan<byte> header)
    {
        var extension = Path.GetExtension(blobName).ToLowerInvariant();
        return extension switch
        {
            ".xlsx" => header.Length >= 4 &&
                header[0] == 0x50 && header[1] == 0x4B &&
                header[2] == 0x03 && header[3] == 0x04,
            ".jpg" or ".jpeg" => header.Length >= 3 &&
                header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header.Length >= 8 &&
                header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => header.Length >= 6 &&
                (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)),
            ".webp" => header.Length >= 12 &&
                header[..4].SequenceEqual("RIFF"u8) &&
                header[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }
    private static bool IsGeneratedBlobName(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName) ||
            !string.Equals(
                Path.GetFileName(blobName),
                blobName,
                StringComparison.Ordinal))
        {
            return false;
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(blobName);
        return Guid.TryParseExact(nameWithoutExtension, "N", out _);
    }

    public async Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string? contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

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

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(
        string fileUrl)
    {
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.AbsolutePath);
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadStreamingAsync();
        var contentType = response.Value.Details.ContentType;

        return (
            response.Value.Content,
            string.IsNullOrWhiteSpace(contentType)
                ? "application/octet-stream"
                : contentType);
    }

    public async Task DeleteAsync(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.AbsolutePath);
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}