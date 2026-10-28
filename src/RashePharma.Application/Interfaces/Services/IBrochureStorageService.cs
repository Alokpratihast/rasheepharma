namespace RashePharma.Application.Interfaces.Services;

public interface IBrochureStorageService
{
    Task<EmailAttachment> DownloadAsync(CancellationToken cancellationToken = default);
}
