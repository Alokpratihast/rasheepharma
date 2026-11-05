namespace RashePharma.Application.Interfaces.Services;

public interface IEmailJobSender
{
    Task<string> SendAsync(
        Guid emailJobId,
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
