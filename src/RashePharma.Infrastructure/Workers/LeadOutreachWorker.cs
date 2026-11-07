using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure.Workers;

/// <summary>
/// Turns queued campaign recipients into durable EmailJob + Outbox records.
/// It does not contact the mail provider; the EmailWorker owns delivery.
/// </summary>
public sealed class LeadOutreachWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<LeadOutreachWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
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
                await Task.Delay(processed ? TimeSpan.FromMilliseconds(200) : TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Lead outreach job producer encountered an error.");
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
        await RecoverStaleClaimsAsync(ct);

        int recipientId;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var recipient = await db.LeadOutreachRecipients
                .FromSqlRaw("SELECT TOP (1) * FROM [LeadOutreachRecipients] WITH (UPDLOCK, READPAST, ROWLOCK) WHERE [Status] = 'Pending' ORDER BY [CreatedAtUtc], [Id]")
                .FirstOrDefaultAsync(ct);
            if (recipient is null) return false;

            recipient.Status = "Processing";
            recipient.ProcessingAtUtc = DateTime.UtcNow;
            recipient.AttemptCount++;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            recipientId = recipient.Id;
        }

        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<ApplicationDbContext>();
            var jobs = services.GetRequiredService<IEmailJobService>();
            var jobRepository = services.GetRequiredService<IEmailJobRepository>();
            var recipient = await db.LeadOutreachRecipients
                .Include(row => row.Lead)
                .Include(row => row.Campaign)
                .FirstOrDefaultAsync(row => row.Id == recipientId, ct);
            if (recipient is null) return true;

            if (!IsEligible(recipient.Lead))
            {
                recipient.Status = "Skipped";
                recipient.ProcessingAtUtc = null;
                recipient.ErrorMessage = "Lead is no longer eligible for email outreach.";
                await db.SaveChangesAsync(ct);
                await RefreshCampaignAsync(db, recipient.CampaignId, ct);
                return true;
            }

            var existing = await jobRepository.GetByLeadOutreachRecipientIdAsync(recipient.Id);
            if (existing is not null)
            {
                ApplyJobState(recipient, existing);
                await db.SaveChangesAsync(ct);
                await RefreshCampaignAsync(db, recipient.CampaignId, ct);
                return true;
            }

            var origin = GetFrontendOrigin();
            var secret = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT signing key is not configured for outreach email links.");

            var unsubscribeToken = LeadUnsubscribeToken.Create(recipient.Id, recipient.LeadId, secret);
            var responseToken = LeadOutreachResponseToken.Create(recipient.Id, recipient.LeadId, DateTime.UtcNow.AddDays(30), secret);
            var originUrl = origin.ToString().TrimEnd('/');
            var unsubscribeUrl = $"{originUrl}/unsubscribe?token={Uri.EscapeDataString(unsubscribeToken)}";
            var responseUrl = $"{originUrl}/outreach/respond?token={Uri.EscapeDataString(responseToken)}";
            var payload = new EmailJobMessagePayload
            {
                ToName = recipient.Lead.Name,
                Subject = recipient.Campaign.Subject,
                HtmlBody = RenderTemplate(recipient.Campaign, recipient.Lead.Name, unsubscribeUrl, responseUrl, origin),
                AttachBrochure = recipient.Campaign.AttachBrochure
            };
            var serializedPayload = JsonSerializer.Serialize(payload, JsonOptions);
            var job = await jobs.CreateJobAsync(new EmailJob
            {
                IdempotencyKey = $"lead-outreach-recipient:{recipient.Id}",
                Category = EmailCategory.Bulk,
                EmailType = "LeadOutreach",
                LeadOutreachRecipientId = recipient.Id,
                RecipientEmail = recipient.Lead.Email!,
                TemplateKey = "lead-outreach",
                TemplateVersion = 1,
                PayloadJson = serializedPayload,
                CorrelationId = $"lead-campaign:{recipient.CampaignId}:recipient:{recipient.Id}",
                Status = EmailJobStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            }, ct);

            ApplyJobState(recipient, job);
            await db.SaveChangesAsync(ct);
            await RefreshCampaignAsync(db, recipient.CampaignId, ct);
            logger.LogInformation("Queued EmailJob {EmailJobId} for outreach campaign {CampaignId}, recipient {RecipientId}.", job.Id, recipient.CampaignId, recipient.Id);
        }

        return true;
    }

    private async Task RecoverStaleClaimsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var jobs = scope.ServiceProvider.GetRequiredService<IEmailJobRepository>();
        var cutoff = DateTime.UtcNow.AddMinutes(-10);
        var stale = await db.LeadOutreachRecipients
            .Where(row => row.Status == "Processing" && row.ProcessingAtUtc < cutoff)
            .OrderBy(row => row.Id)
            .Take(50)
            .ToListAsync(ct);
        foreach (var recipient in stale)
        {
            var existing = await jobs.GetByLeadOutreachRecipientIdAsync(recipient.Id);
            if (existing is null)
            {
                recipient.Status = "Pending";
                recipient.ErrorMessage = "Queue creation was interrupted; safe to retry using the same idempotency key.";
            }
            else
            {
                ApplyJobState(recipient, existing);
            }
            recipient.ProcessingAtUtc = null;
        }
        if (stale.Count > 0) await db.SaveChangesAsync(ct);
    }

    private static bool IsEligible(ExternalLead lead) =>
        !lead.IsSuppressed &&
        string.Equals(lead.PermissionStatus, "Allowed", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(lead.Email);

    private static void ApplyJobState(LeadOutreachRecipient recipient, EmailJob job)
    {
        recipient.Status = job.Status switch
        {
            EmailJobStatus.ProviderAccepted => "Sent",
            EmailJobStatus.FailedPermanent or EmailJobStatus.DeadLettered or EmailJobStatus.OutcomeUnknown => "Failed",
            EmailJobStatus.Suppressed or EmailJobStatus.Cancelled => "Skipped",
            _ => "Queued"
        };
        recipient.SentAtUtc = job.AcceptedAtUtc;
        recipient.ProcessingAtUtc = null;
        recipient.ErrorMessage = recipient.Status switch
        {
            "Failed" => "Delivery outcome needs review in the email provider/job log before any retry.",
            "Skipped" => "Email job is suppressed or cancelled.",
            _ => null
        };
    }

    private async Task RefreshCampaignAsync(ApplicationDbContext db, int campaignId, CancellationToken ct)
    {
        var campaign = await db.LeadOutreachCampaigns.FirstOrDefaultAsync(row => row.Id == campaignId, ct);
        if (campaign is null) return;
        campaign.SentCount = await db.LeadOutreachRecipients.CountAsync(row => row.CampaignId == campaignId && row.Status == "Sent", ct);
        campaign.FailedCount = await db.LeadOutreachRecipients.CountAsync(row => row.CampaignId == campaignId && row.Status == "Failed", ct);
        var hasRemaining = await db.LeadOutreachRecipients.AnyAsync(row => row.CampaignId == campaignId &&
            (row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued"), ct);
        campaign.Status = hasRemaining ? "Queued" : campaign.FailedCount > 0 ? "CompletedWithErrors" : "Completed";
        campaign.CompletedAtUtc = hasRemaining ? null : DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private Uri GetFrontendOrigin()
    {
        var baseUrl = configuration["Frontend:BaseUrl"] ?? configuration["Company:WebsiteUrl"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var origin) ||
            (origin.Scheme != Uri.UriSchemeHttps && !IsLocalHttp(origin)))
            throw new InvalidOperationException("Frontend:BaseUrl must be HTTPS for outreach response links.");
        return origin;
    }

    private static string RenderTemplate(LeadOutreachCampaign campaign, string recipientName, string unsubscribeUrl, string responseUrl, Uri websiteUrl)
    {
        var safeName = WebUtility.HtmlEncode(recipientName);
        var logoUrl = WebUtility.HtmlEncode(new Uri(websiteUrl, "/images/Rashelifescience.png").AbsoluteUri);
        var paragraphs = campaign.BodyText.Split("\n", StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 0)
            .Select(line => $"<p style=\"margin:0 0 14px;color:#334155;line-height:1.6\">{WebUtility.HtmlEncode(line)}</p>");
        var content = string.Join("\n", paragraphs);
        return $"""
            <!doctype html><html><body style="margin:0;background:#f1f5f9;font-family:Arial,sans-serif;color:#0f172a">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:32px 12px"><tr><td align="center">
            <table role="presentation" width="600" cellspacing="0" cellpadding="0" style="max-width:600px;background:#fff;border-radius:14px;overflow:hidden">
            <tr><td style="background:#064e3b;padding:24px 32px;color:#fff"><table role="presentation" cellspacing="0" cellpadding="0"><tr><td style="background:#fff;padding:8px 12px;border-radius:8px"><a href="{WebUtility.HtmlEncode(websiteUrl.ToString().TrimEnd('/'))}" style="text-decoration:none"><img src="{logoUrl}" alt="Rashe Lifesciences Pvt. Ltd. — A Pharmaceutical Company" width="210" style="display:block;width:210px;max-width:100%;height:auto;border:0"></a></td></tr><tr><td style="padding-top:12px;font-size:12px;color:#a7f3d0">Healthcare & pharmaceutical solutions</td></tr></table></td></tr>
            <tr><td style="padding:32px"><p style="margin:0 0 18px">Hello {safeName},</p>{content}<p style="margin:24px 0 0;color:#334155">Regards,<br><strong>Rashe Lifesciences Team</strong></p><p style="margin:24px 0 0"><a href="{WebUtility.HtmlEncode(responseUrl)}" style="display:inline-block;background:#047857;color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;font-weight:bold">Reply to this message</a></p></td></tr>
            <tr><td style="border-top:1px solid #e2e8f0;padding:18px 32px;color:#64748b;font-size:12px">You are receiving this business email because your contact is recorded in our lead system. <a href="{WebUtility.HtmlEncode(unsubscribeUrl)}" style="color:#047857">Unsubscribe</a> · <a href="{WebUtility.HtmlEncode(websiteUrl.ToString().TrimEnd('/'))}" style="color:#047857">Website</a></td></tr>
            </table></td></tr></table></body></html>
            """;
    }

    private static bool IsLocalHttp(Uri uri) => uri.Scheme == Uri.UriSchemeHttp &&
        (uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || uri.Host == "127.0.0.1");
}
