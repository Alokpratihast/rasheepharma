namespace RashePharma.Application.Interfaces.Services;

/// <summary>
/// Metadata used to request upload permissions or finalize a staged file.
/// FileType is either "Excel" or "Image" and is validated by the service.
/// </summary>
public sealed record BulkUploadFileDescriptor(
    string FileName,
    string? ContentType,
    long FileSize,
    string FileType);

/// <summary>
/// Upload instructions returned to the browser. UploadUrl contains a short-lived
/// credential for one generated staging blob; never log or persist that URL.
/// </summary>
public sealed record BulkUploadUploadTarget(
    string FileName,
    string FileType,
    long FileSize,
    string BlobName,
    string UploadUrl);

public sealed record BulkUploadStagedFileInput(
    string FileName,
    string? ContentType,
    long FileSize,
    string FileType,
    string BlobName);