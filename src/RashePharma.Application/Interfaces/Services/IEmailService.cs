namespace RashePharma.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody);

    Task SendOrderInvoiceAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        byte[] invoicePdf,
        string fileName);
}