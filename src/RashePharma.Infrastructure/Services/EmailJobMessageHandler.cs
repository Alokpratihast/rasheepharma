using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.Email;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Configurations;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Services;

public enum EmailJobMessageResult
{
    Completed,
    Busy,
    NotFound,
    Invalid
}

public sealed class EmailJobMessageHandler(
    ApplicationDbContext db,
    IEmailService emailService,
    IOptions<EmailJobConsumerOptions> options,
    ILogger<EmailJobMessageHandler> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EmailJobMessageResult> HandleAsync(
        Guid emailJobId,
        CancellationToken cancellationToken)
    {
        if (emailJobId == Guid.Empty)
            return EmailJobMessageResult.Invalid;

        EmailJob? job;
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            job = await db.EmailJobs
                .FromSqlInterpolated($"SELECT * FROM [EmailJobs] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {emailJobId}")
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
                return EmailJobMessageResult.NotFound;

            if (IsTerminal(job.Status))
                return EmailJobMessageResult.Completed;

            var now = DateTime.UtcNow;
            if (job.Status == EmailJobStatus.Processing &&
                job.ProcessingLeaseUntilUtc > now)
            {
                return EmailJobMessageResult.Busy;
            }

            // If a previous worker lost its lock while SMTP was in progress, the
            // provider may already have accepted the email. Never resend it blindly.
            if (job.Status == EmailJobStatus.Processing)
            {
                job.Status = EmailJobStatus.OutcomeUnknown;
                job.ProcessingLeaseUntilUtc = null;
                job.UpdatedAtUtc = now;
                job.LastErrorCode = "WORKER_LEASE_EXPIRED";
                job.LastErrorMessage = "Delivery outcome could not be confirmed; check the provider log before retrying.";
                await MarkOutreachFailedAsync(job, job.LastErrorMessage, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                await RefreshCampaignForJobAsync(job, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return EmailJobMessageResult.Completed;
            }

            if (job.Status == EmailJobStatus.RetryScheduled &&
                job.NextAttemptAtUtc > now)
            {
                return EmailJobMessageResult.Busy;
            }

            if (job.Status is not (EmailJobStatus.Pending or EmailJobStatus.Queued or EmailJobStatus.RetryScheduled))
                return EmailJobMessageResult.Completed;

            job.Status = EmailJobStatus.Processing;
            job.AttemptCount++;
            job.ProcessingLeaseUntilUtc = now.AddMinutes(options.Value.ProcessingLeaseMinutes);
            job.NextAttemptAtUtc = null;
            job.UpdatedAtUtc = now;
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        EmailJobMessagePayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<EmailJobMessagePayload>(job.PayloadJson, JsonOptions);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Email job {EmailJobId} has an invalid payload.", job.Id);
            await MarkPermanentFailureAsync(job, "INVALID_PAYLOAD", cancellationToken);
            return EmailJobMessageResult.Invalid;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.HtmlBody))
        {
            await MarkPermanentFailureAsync(job, "INVALID_PAYLOAD", cancellationToken);
            return EmailJobMessageResult.Invalid;
        }

        if (job.LeadOutreachRecipientId.HasValue)
        {
            var recipient = await db.LeadOutreachRecipients
                .Include(row => row.Lead)
                .FirstOrDefaultAsync(row => row.Id == job.LeadOutreachRecipientId.Value, cancellationToken);

            if (recipient is null || recipient.Lead.IsSuppressed ||
                recipient.Lead.PermissionStatus != "Allowed" ||
                string.IsNullOrWhiteSpace(recipient.Lead.Email))
            {
                await MarkSuppressedAsync(job, recipient, cancellationToken);
                return EmailJobMessageResult.Completed;
            }
        }

        try
        {
            await emailService.SendAsync(job.RecipientEmail, payload.ToName, payload.Subject, payload.HtmlBody);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The queue lock will be released and the lease-expiry path will
            // convert this to OutcomeUnknown rather than risking a duplicate.
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Email job {EmailJobId} failed during provider delivery.", job.Id);
            job.Status = EmailJobStatus.OutcomeUnknown;
            job.ProcessingLeaseUntilUtc = null;
            job.UpdatedAtUtc = DateTime.UtcNow;
            job.LastErrorCode = "PROVIDER_OUTCOME_UNKNOWN";
            job.LastErrorMessage = "Delivery outcome could not be confirmed; check the provider log before retrying.";
            await MarkOutreachFailedAsync(job, job.LastErrorMessage, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await RefreshCampaignForJobAsync(job, cancellationToken);
            return EmailJobMessageResult.Completed;
        }

        // Persist provider acceptance outside the send exception handler. If a
        // later SQL operation fails, Service Bus can redeliver; ProviderAccepted
        // is terminal, so that redelivery will not send a duplicate email.
        await MarkAcceptedAsync(job, cancellationToken);

        return EmailJobMessageResult.Completed;
    }

    private async Task MarkAcceptedAsync(EmailJob job, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        job.Status = EmailJobStatus.ProviderAccepted;
        job.AcceptedAtUtc = now;
        job.ProcessingLeaseUntilUtc = null;
        job.UpdatedAtUtc = now;
        job.LastErrorCode = null;
        job.LastErrorMessage = null;
        db.EmailDeliveryEvents.Add(new EmailDeliveryEvent
        {
            EmailJobId = job.Id,
            Provider = "Brevo",
            EventType = "Accepted",
            OccurredAtUtc = now,
            ReceivedAtUtc = now
        });

        if (job.LeadOutreachRecipientId.HasValue)
        {
            var recipient = await db.LeadOutreachRecipients
                .Include(row => row.Lead)
                .Include(row => row.Campaign)
                .FirstOrDefaultAsync(row => row.Id == job.LeadOutreachRecipientId.Value, ct);
            if (recipient is not null)
            {
                recipient.Status = "Sent";
                recipient.SentAtUtc = now;
                recipient.ProcessingAtUtc = null;
                recipient.ErrorMessage = null;
                recipient.Lead.UpdatedAtUtc = now;
                db.LeadActivities.Add(new LeadActivity
                {
                    LeadId = recipient.LeadId,
                    Type = "OutreachSent",
                    Details = $"Campaign {recipient.CampaignId} ({recipient.Campaign.Subject}) accepted by the email provider.",
                    Actor = recipient.Campaign.CreatedBy
                });
            }
        }

        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task MarkSuppressedAsync(EmailJob job, LeadOutreachRecipient? recipient, CancellationToken ct)
    {
        job.Status = EmailJobStatus.Suppressed;
        job.ProcessingLeaseUntilUtc = null;
        job.UpdatedAtUtc = DateTime.UtcNow;
        if (recipient is not null)
        {
            recipient.Status = "Skipped";
            recipient.ProcessingAtUtc = null;
            recipient.ErrorMessage = "Lead is no longer eligible for email outreach.";
        }
        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task MarkOutreachFailedAsync(EmailJob job, string message, CancellationToken ct)
    {
        if (!job.LeadOutreachRecipientId.HasValue)
            return;
        var recipient = await db.LeadOutreachRecipients
            .FirstOrDefaultAsync(row => row.Id == job.LeadOutreachRecipientId.Value, ct);
        if (recipient is null)
            return;
        recipient.Status = "Failed";
        recipient.ProcessingAtUtc = null;
        recipient.ErrorMessage = message;
    }

    private async Task MarkPermanentFailureAsync(EmailJob job, string code, CancellationToken ct)
    {
        job.Status = EmailJobStatus.FailedPermanent;
        job.ProcessingLeaseUntilUtc = null;
        job.UpdatedAtUtc = DateTime.UtcNow;
        job.LastErrorCode = code;
        job.LastErrorMessage = "The email job payload is invalid and requires correction.";
        await MarkOutreachFailedAsync(job, job.LastErrorMessage, ct);
        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task RefreshCampaignForJobAsync(EmailJob job, CancellationToken ct)
    {
        if (!job.LeadOutreachRecipientId.HasValue) return;
        var campaignId = await db.LeadOutreachRecipients
            .Where(row => row.Id == job.LeadOutreachRecipientId.Value)
            .Select(row => (int?)row.CampaignId)
            .FirstOrDefaultAsync(ct);
        if (!campaignId.HasValue) return;
        var campaign = await db.LeadOutreachCampaigns.FirstOrDefaultAsync(row => row.Id == campaignId, ct);
        if (campaign is null)
            return;
        campaign.SentCount = await db.LeadOutreachRecipients.CountAsync(
            row => row.CampaignId == campaignId && row.Status == "Sent", ct);
        campaign.FailedCount = await db.LeadOutreachRecipients.CountAsync(
            row => row.CampaignId == campaignId && row.Status == "Failed", ct);
        var hasRemaining = await db.LeadOutreachRecipients.AnyAsync(
            row => row.CampaignId == campaignId &&
                (row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued"), ct);
        campaign.Status = hasRemaining ? "Queued" : campaign.FailedCount > 0 ? "CompletedWithErrors" : "Completed";
        campaign.CompletedAtUtc = hasRemaining ? null : DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private static bool IsTerminal(EmailJobStatus status) => status is
        EmailJobStatus.OutcomeUnknown or EmailJobStatus.ProviderAccepted or
        EmailJobStatus.FailedPermanent or EmailJobStatus.DeadLettered or
        EmailJobStatus.Suppressed or EmailJobStatus.Cancelled;
}
