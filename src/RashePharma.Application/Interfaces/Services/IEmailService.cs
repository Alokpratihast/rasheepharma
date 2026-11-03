
namespace RashePharma.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        EmailAttachment? attachment = null,
        CancellationToken cancellationToken = default);

    Task SendOrderInvoiceAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[] invoicePdf,
        string fileName);
}
