using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Data;
using System.Text.Json;

namespace RashePharma.Infrastructure.Services;

public enum BrevoWebhookProcessResult { Processed, Duplicate, Unmatched }

/// <summary>Stores Brevo events idempotently and applies CRM delivery/suppression state.</summary>
public sealed class BrevoWebhookProcessor(ApplicationDbContext db)
{
    public async Task<BrevoWebhookProcessResult> ProcessAsync(
        JsonElement payload,
        CancellationToken cancellationToken = default)
    {
        if (!BrevoWebhookEventParser.TryParse(payload, out var webhookEvent) || webhookEvent is null)
            throw new ArgumentException("The Brevo webhook event is invalid.", nameof(payload));

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var duplicate = await db.EmailDeliveryEvents.AnyAsync(
            row => row.Provider == "Brevo" && row.ProviderEventId == webhookEvent.ProviderEventId,
            cancellationToken);
        if (duplicate)
        {
            await transaction.CommitAsync(cancellationToken);
            return BrevoWebhookProcessResult.Duplicate;
        }

        EmailJob? job = null;
        if (Guid.TryParse(webhookEvent.CustomCorrelation, out var emailJobId) && emailJobId != Guid.Empty)
        {
            job = await db.EmailJobs
                .FromSqlInterpolated($"SELECT * FROM [EmailJobs] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {emailJobId}")
                .FirstOrDefaultAsync(cancellationToken);
        }
        if (job is null && !string.IsNullOrWhiteSpace(webhookEvent.ProviderMessageId))
        {
            job = await db.EmailJobs
                .FromSqlInterpolated($"SELECT * FROM [EmailJobs] WITH (UPDLOCK, ROWLOCK) WHERE [ProviderMessageId] = {webhookEvent.ProviderMessageId}")
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (job is null || !string.Equals(
                NormalizeEmail(job.RecipientEmail), NormalizeEmail(webhookEvent.Email),
                StringComparison.Ordinal))
        {
            await transaction.CommitAsync(cancellationToken);
            return BrevoWebhookProcessResult.Unmatched;
        }

        var now = DateTime.UtcNow;
        db.EmailDeliveryEvents.Add(new EmailDeliveryEvent
        {
            EmailJobId = job.Id,
            Provider = "Brevo",
            ProviderEventId = webhookEvent.ProviderEventId,
            EventType = webhookEvent.EventName[..Math.Min(webhookEvent.EventName.Length, 60)],
            OccurredAtUtc = webhookEvent.OccurredAtUtc,
            ReceivedAtUtc = now,
            RawPayloadJson = webhookEvent.RawPayloadJson
        });

        if (webhookEvent.DeliveryStatus.HasValue)
            job.DeliveryStatus = MergeStatus(job.DeliveryStatus, webhookEvent.DeliveryStatus.Value);

        if (job.LeadOutreachRecipientId.HasValue)
        {
            var recipient = await db.LeadOutreachRecipients
                .Include(row => row.Lead)
                .Include(row => row.Campaign)
                .FirstOrDefaultAsync(row => row.Id == job.LeadOutreachRecipientId.Value, cancellationToken);
            if (recipient is not null)
            {
                var eventLabel = webhookEvent.DeliveryStatus?.ToString() ?? webhookEvent.EventName;
                db.LeadActivities.Add(new LeadActivity
                {
                    LeadId = recipient.LeadId,
                    Type = $"Email{eventLabel}",
                    Details = $"Campaign {recipient.CampaignId} ({recipient.Campaign.Name}) email event: {eventLabel}." +
                              (string.IsNullOrWhiteSpace(webhookEvent.Reason) ? string.Empty : $" Reason: {webhookEvent.Reason[..Math.Min(webhookEvent.Reason.Length, 300)]}"),
                    Actor = "Brevo"
                });

                if (webhookEvent.SuppressionType is not null)
                    await SuppressLeadAndAddressAsync(recipient.Lead, webhookEvent, now, cancellationToken);
            }
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return BrevoWebhookProcessResult.Processed;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            return BrevoWebhookProcessResult.Duplicate;
        }
    }

    private async Task SuppressLeadAndAddressAsync(
        ExternalLead lead,
        BrevoWebhookEvent webhookEvent,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(webhookEvent.Email);
        var exists = await db.EmailSuppressions.AnyAsync(
            row => row.EmailNormalized == normalizedEmail &&
                   row.SuppressionType == webhookEvent.SuppressionType &&
                   row.LiftedAtUtc == null,
            cancellationToken);
        if (!exists)
        {
            db.EmailSuppressions.Add(new EmailSuppression
            {
                EmailNormalized = normalizedEmail,
                SuppressionType = webhookEvent.SuppressionType!,
                Source = "BrevoWebhook",
                Reason = string.IsNullOrWhiteSpace(webhookEvent.Reason)
                    ? $"Brevo event: {webhookEvent.EventName}"
                    : webhookEvent.Reason[..Math.Min(webhookEvent.Reason.Length, 500)],
                CreatedAtUtc = now
            });
        }

        if (lead.IsSuppressed)
            return;

        lead.IsSuppressed = true;
        lead.SuppressedAtUtc = now;
        lead.UpdatedAtUtc = now;
        db.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            Type = "Suppressed",
            Details = $"Address suppressed from outreach after Brevo {webhookEvent.EventName} event.",
            Actor = "Brevo"
        });
    }

    private static EmailDeliveryStatus MergeStatus(
        EmailDeliveryStatus current,
        EmailDeliveryStatus incoming)
    {
        if (incoming is EmailDeliveryStatus.Bounced or EmailDeliveryStatus.Complained or EmailDeliveryStatus.Unsubscribed)
            return incoming;
        if (current is EmailDeliveryStatus.Bounced or EmailDeliveryStatus.Complained or EmailDeliveryStatus.Unsubscribed)
            return current;
        return (int)incoming > (int)current ? incoming : current;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };
}
