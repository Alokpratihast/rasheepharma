using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure.Workers;

public sealed class LeadOutreachWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<LeadOutreachWorker> logger) : BackgroundService
{
    private DateTime _lastPreviewCleanupUtc = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (DateTime.UtcNow - _lastPreviewCleanupUtc >= TimeSpan.FromHours(1))
                {
                    await DeleteExpiredImportPreviewsAsync(stoppingToken);
                    _lastPreviewCleanupUtc = DateTime.UtcNow;
                }
                var processed = await ProcessOneAsync(stoppingToken);
                if (!processed) await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                else await Task.Delay(TimeSpan.FromMilliseconds(200), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Lead outreach worker encountered an error.");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    private async Task DeleteExpiredImportPreviewsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var count = await db.LeadImportBatches
            .Where(batch => batch.Status == "Preview" && batch.CreatedAtUtc < cutoff)
            .ExecuteDeleteAsync(ct);
        if (count > 0) logger.LogInformation("Expired {Count} unconfirmed lead import previews.", count);
    }

    private async Task<bool> ProcessOneAsync(CancellationToken ct)
    {
        LeadOutreachRecipient? recipient;
        await using (var claimScope = scopeFactory.CreateAsyncScope())
        {
            var db = claimScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var staleBefore = DateTime.UtcNow.AddMinutes(-10);
            await db.LeadOutreachRecipients.Where(row => row.Status == "Processing" && row.ProcessingAtUtc < staleBefore)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(row => row.Status, "Failed")
                    .SetProperty(row => row.ErrorMessage, "Worker interrupted; verify provider logs before retrying.")
                    .SetProperty(row => row.ProcessingAtUtc, (DateTime?)null), ct);
            recipient = await db.LeadOutreachRecipients
                .FromSqlRaw("SELECT TOP (1) * FROM [LeadOutreachRecipients] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Status] = 'Pending' ORDER BY [CreatedAtUtc], [Id]")
                .FirstOrDefaultAsync(ct);
            if (recipient is null) return false;
            recipient.Status = "Processing";
            recipient.ProcessingAtUtc = DateTime.UtcNow;
            recipient.AttemptCount++;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }

        using (var sendScope = scopeFactory.CreateScope())
        {
            var db = sendScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = sendScope.ServiceProvider.GetRequiredService<IEmailService>();
            var item = await db.LeadOutreachRecipients.Include(row => row.Lead).Include(row => row.Campaign)
                .FirstOrDefaultAsync(row => row.Id == recipient.Id, ct);
            if (item is null) return true;

            try
            {
                if (item.Lead.IsSuppressed || item.Lead.PermissionStatus != "Allowed" || string.IsNullOrWhiteSpace(item.Lead.Email))
                {
                    item.Status = "Skipped";
                    item.ErrorMessage = "Lead is no longer eligible for email outreach.";
                }
                else
                {
                    var baseUrl = configuration["Frontend:BaseUrl"] ?? configuration["Company:WebsiteUrl"];
                    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var origin) ||
                        (origin.Scheme != Uri.UriSchemeHttps && !IsLocalHttp(origin)))
                        throw new InvalidOperationException("Frontend:BaseUrl must be a valid HTTPS URL for unsubscribe links.");

                    var secret = configuration["Jwt:Key"];
                    if (string.IsNullOrWhiteSpace(secret)) throw new InvalidOperationException("JWT signing key is not configured.");
                    var token = LeadUnsubscribeToken.Create(item.Id, item.LeadId, secret);
                    var unsubscribeUrl = $"{origin.ToString().TrimEnd('/')}/unsubscribe?token={Uri.EscapeDataString(token)}";
                    var body = RenderTemplate(item.Campaign, item.Lead.Name, unsubscribeUrl, origin.ToString().TrimEnd('/'));
                    await emailService.SendAsync(item.Lead.Email, item.Lead.Name, item.Campaign.Subject, body);
                    item.Status = "Sent";
                    item.SentAtUtc = DateTime.UtcNow;
                    item.Lead.UpdatedAtUtc = DateTime.UtcNow;
                    db.LeadActivities.Add(new LeadActivity
                    {
                        LeadId = item.LeadId,
                        Type = "OutreachSent",
                        Details = $"Campaign {item.CampaignId} ({item.Campaign.Subject}) sent.",
                        Actor = item.Campaign.CreatedBy
                    });
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Do not retry SMTP errors automatically: a timeout can occur after provider acceptance.
                item.Status = "Failed";
                item.ErrorMessage = "Delivery failed; check the email provider log before retrying.";
                logger.LogWarning(exception, "Outreach delivery failed. CampaignId={CampaignId} RecipientId={RecipientId}", item.CampaignId, item.Id);
            }

            // Persist the recipient terminal status before querying campaign totals.
            item.ProcessingAtUtc = null;
            await db.SaveChangesAsync(ct);

            item.Campaign.SentCount = await db.LeadOutreachRecipients.CountAsync(
                row => row.CampaignId == item.CampaignId && row.Status == "Sent", ct);
            item.Campaign.FailedCount = await db.LeadOutreachRecipients.CountAsync(
                row => row.CampaignId == item.CampaignId && row.Status == "Failed", ct);
            var remaining = await db.LeadOutreachRecipients.AnyAsync(row => row.CampaignId == item.CampaignId &&
                (row.Status == "Pending" || row.Status == "Processing"), ct);
            if (!remaining)
            {
                item.Campaign.Status = item.Campaign.FailedCount > 0 ? "CompletedWithErrors" : "Completed";
                item.Campaign.CompletedAtUtc = DateTime.UtcNow;
            }
            else
            {
                item.Campaign.Status = "Queued";
                item.Campaign.CompletedAtUtc = null;
            }
            await db.SaveChangesAsync(ct);
        }
        return true;
    }

    private static string RenderTemplate(LeadOutreachCampaign campaign, string recipientName, string unsubscribeUrl, string websiteUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var paragraphs = campaign.BodyText.Split("\n", StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 0)
            .Select(line => $"<p style=\"margin:0 0 14px;color:#334155;line-height:1.6\">{WebUtility.HtmlEncode(line)}</p>");
        var content = string.Join("\n", paragraphs);
        return $"""
            <!doctype html><html><body style="margin:0;background:#f1f5f9;font-family:Arial,sans-serif;color:#0f172a">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 12px"><tr><td align="center">
            <table role="presentation" width="600" cellspacing="0" cellpadding="0" style="max-width:600px;background:#fff;border-radius:14px;overflow:hidden">
            <tr><td style="background:#064e3b;padding:24px 32px;color:#fff"><strong style="font-size:20px">Rashe Lifesciences</strong><br><span style="font-size:12px;color:#a7f3d0">Healthcare & pharmaceutical solutions</span></td></tr>
            <tr><td style="padding:32px"><p style="margin:0 0 18px">Hello {safeName},</p>{content}<p style="margin:24px 0 0;color:#334155">Regards,<br><strong>Rashe Lifesciences Team</strong></p></td></tr>
            <tr><td style="border-top:1px solid #e2e8f0;padding:18px 32px;color:#64748b;font-size:12px">You are receiving this business email because your contact is recorded in our lead system. <a href="{WebUtility.HtmlEncode(unsubscribeUrl)}" style="color:#047857">Unsubscribe</a> · <a href="{WebUtility.HtmlEncode(websiteUrl)}" style="color:#047857">Website</a></td></tr>
            </table></td></tr></table></body></html>
            """;
    }

    private static bool IsLocalHttp(Uri uri) => uri.Scheme == Uri.UriSchemeHttp &&
        (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host == "127.0.0.1");
}
