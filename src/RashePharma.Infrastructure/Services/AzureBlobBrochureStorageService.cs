using Azure.Storage.Blobs;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public sealed class AzureBlobBrochureStorageService : IBrochureStorageService
{
    private const string PdfContentType = "application/pdf";
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    private readonly BlobClient _blob;
    private readonly string _fileName;
    private readonly long _maxBytes;

    public AzureBlobBrochureStorageService(
        string connectionString,
        string containerName,
        string blobName,
        long maxBytes)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Azure Storage connection string is required.", nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(blobName))
        {
            throw new ArgumentException("Brochure container and blob names must be configured.");
        }

        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes), "Maximum brochure size must be positive.");
        }

        var blobService = new BlobServiceClient(connectionString);
        _blob = blobService.GetBlobContainerClient(containerName).GetBlobClient(blobName);
        _fileName = Path.GetFileName(blobName);
        _maxBytes = maxBytes;
    }

    public async Task<EmailAttachment> DownloadAsync(CancellationToken cancellationToken = default)
    {
        var properties = await _blob.GetPropertiesAsync(cancellationToken: cancellationToken);
        var size = properties.Value.ContentLength;
        if (size <= 0 || size > _maxBytes)
        {
            throw new InvalidOperationException(
                $"Configured brochure size must be between 1 byte and {_maxBytes} bytes.");
        }

        if (!string.IsNullOrWhiteSpace(properties.Value.ContentType) &&
            !string.Equals(properties.Value.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The configured quotation brochure is not marked as a PDF.");
        }

        var download = await _blob.DownloadContentAsync(cancellationToken);
        var content = download.Value.Content.ToArray();
        if (content.Length < PdfSignature.Length ||
            !content.AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature))
        {
            throw new InvalidOperationException("The configured quotation brochure does not contain a valid PDF signature.");
        }

        return new EmailAttachment(_fileName, PdfContentType, content);
    }
}
