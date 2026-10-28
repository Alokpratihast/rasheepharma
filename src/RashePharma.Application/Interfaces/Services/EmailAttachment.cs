namespace RashePharma.Application.Interfaces.Services;

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    byte[] Content);
