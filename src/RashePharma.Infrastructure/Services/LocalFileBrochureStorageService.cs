using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public sealed class LocalFileBrochureStorageService : IBrochureStorageService
{
    private const string PdfContentType = "application/pdf";
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private readonly string _path;
    private readonly string _fileName;
    private readonly long _maxBytes;

    public LocalFileBrochureStorageService(
        LocalFileStorageRoot storage,
        string fileName,
        long maxBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
            throw new ArgumentException("Brochure file name must be a simple file name.", nameof(fileName));
        if (maxBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maxBytes));

        _fileName = fileName;
        _path = storage.GetPath("brochures", fileName);
        _maxBytes = maxBytes;
    }

    public async Task<EmailAttachment> DownloadAsync(CancellationToken cancellationToken = default)
    {
        var info = new FileInfo(_path);
        if (!info.Exists) throw new FileNotFoundException("Configured quotation brochure was not found.", _path);
        if (info.Length <= 0 || info.Length > _maxBytes)
            throw new InvalidOperationException($"Configured brochure size must be between 1 byte and {_maxBytes} bytes.");

        var content = new byte[checked((int)info.Length)];
        await using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await stream.ReadExactlyAsync(content, cancellationToken);
        }

        if (content.Length < PdfSignature.Length ||
            !content.AsSpan(0, PdfSignature.Length).SequenceEqual(PdfSignature))
            throw new InvalidOperationException("Configured quotation brochure is not a valid PDF.");

        return new EmailAttachment(_fileName, PdfContentType, content);
    }
}
