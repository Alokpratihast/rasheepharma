using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using RashePharma.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace RashePharma.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody)
    {
        var smtpHost =
            _configuration["Brevo:SmtpHost"];

        var smtpPort =
            _configuration.GetValue<int>(
                "Brevo:SmtpPort");

        var smtpUsername =
            _configuration["Brevo:SmtpUsername"];

        var smtpPassword =
            _configuration["Brevo:SmtpPassword"];

        var fromEmail =
            _configuration["Brevo:FromEmail"];

        var fromName =
            _configuration["Brevo:FromName"];

        if (string.IsNullOrWhiteSpace(smtpHost) ||
            string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "Brevo SMTP configuration is missing.");
        }

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                fromName ?? "Rashe Pharma",
                fromEmail));

        message.To.Add(
            new MailboxAddress(
                toName,
                toEmail));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            smtpHost,
            smtpPort == 0 ? 587 : smtpPort,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            smtpUsername,
            smtpPassword);

        await smtp.SendAsync(message);

        await smtp.DisconnectAsync(true);
    }
}