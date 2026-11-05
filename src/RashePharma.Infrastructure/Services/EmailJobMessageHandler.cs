
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

public sealed record EmailJobMessageHandlingResult(
    EmailJobMessageResult Result,
    DateTime? RetryAtUtc = null);

public sealed class EmailJobMessageHandler(
    ApplicationDbContext db,
    IEmailJobSender emailJobSender,
    IEmailOutreachQuotaService quotaService,
    IOptions<EmailJobConsumerOptions> options,
    ILogger<EmailJobMessageHandler> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EmailJobMessageHandlingResult> HandleAsync(
        Guid emailJobId,
        CancellationToken cancellationToken)
    {
        if (emailJobId == Guid.Empty)
        {
            return new(EmailJobMessageResult.Invalid);
        }

        EmailJob? job;

        await using (var transaction =
            await db.Database.BeginTransactionAsync(cancellationToken))
        {
            job = await db.EmailJobs
                .FromSqlInterpolated(
                    $"SELECT * FROM [EmailJobs] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {emailJobId}")
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null)
            {
                return new(EmailJobMessageResult.NotFound);
            }

            if (IsTerminal(job.Status))
            {
                return new(EmailJobMessageResult.Completed);
            }

            var now = DateTime.UtcNow;

            if (job.Status == EmailJobStatus.Processing &&
                job.ProcessingLeaseUntilUtc.HasValue &&
                job.ProcessingLeaseUntilUtc.Value > now)
            {
                return new(
                    EmailJobMessageResult.Busy,
                    job.ProcessingLeaseUntilUtc.Value);
            }

            // An expired processing lease means that SMTP delivery may
            // already have been accepted. Never resend blindly.
            if (job.Status == EmailJobStatus.Processing)
            {
                job.Status = EmailJobStatus.OutcomeUnknown;
                job.ProcessingLeaseUntilUtc = null;
                job.UpdatedAtUtc = now;
                job.LastErrorCode = "WORKER_LEASE_EXPIRED";
                job.LastErrorMessage =
                    "Delivery outcome could not be confirmed; check the provider log before retrying.";

                await MarkOutreachFailedAsync(
                    job,
                    job.LastErrorMessage,
                    cancellationToken);

                await db.SaveChangesAsync(cancellationToken);
                await RefreshCampaignForJobAsync(job, cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new(EmailJobMessageResult.Completed);
            }

            if ((job.Status is EmailJobStatus.RetryScheduled or EmailJobStatus.DeferredQuota) &&
                job.NextAttemptAtUtc.HasValue &&
                job.NextAttemptAtUtc.Value > now)
            {
                return new(
                    EmailJobMessageResult.Busy,
                    job.NextAttemptAtUtc.Value);
            }

            if (job.Status is not (
                EmailJobStatus.Pending or
                EmailJobStatus.Queued or
                EmailJobStatus.RetryScheduled or
                EmailJobStatus.DeferredQuota))
            {
                return new(EmailJobMessageResult.Completed);
            }

            job.Status = EmailJobStatus.Processing;
            job.AttemptCount++;
            job.ProcessingLeaseUntilUtc =
                now.AddMinutes(options.Value.ProcessingLeaseMinutes);
            job.NextAttemptAtUtc = null;
            job.UpdatedAtUtc = now;

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        EmailJobMessagePayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<EmailJobMessagePayload>(
                job.PayloadJson,
                JsonOptions);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception,
                "Email job {EmailJobId} has an invalid payload.",
                job.Id);

            await MarkPermanentFailureAsync(
                job,
                "INVALID_PAYLOAD",
                cancellationToken);

            return new(EmailJobMessageResult.Invalid);
        }

        if (payload is null ||
            string.IsNullOrWhiteSpace(payload.Subject) ||
            string.IsNullOrWhiteSpace(payload.HtmlBody))
        {
            await MarkPermanentFailureAsync(
                job,
                "INVALID_PAYLOAD",
                cancellationToken);

            return new(EmailJobMessageResult.Invalid);
        }

        LeadOutreachRecipient? outreachRecipient = null;
        if (job.LeadOutreachRecipientId.HasValue)
        {
            var normalizedEmail = NormalizeEmail(job.RecipientEmail);
            var globallySuppressed = await db.EmailSuppressions.AnyAsync(
                row => row.EmailNormalized == normalizedEmail && row.LiftedAtUtc == null,
                cancellationToken);
            outreachRecipient = await db.LeadOutreachRecipients
                .Include(row => row.Lead)
                .FirstOrDefaultAsync(row => row.Id == job.LeadOutreachRecipientId.Value, cancellationToken);

            if (outreachRecipient is null || globallySuppressed || outreachRecipient.Lead.IsSuppressed ||
                !string.Equals(outreachRecipient.Lead.PermissionStatus, "Allowed", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(outreachRecipient.Lead.Email) ||
                !string.Equals(
                    NormalizeEmail(outreachRecipient.Lead.Email),
                    normalizedEmail,
                    StringComparison.Ordinal))
            {
                await MarkSuppressedAsync(job, outreachRecipient, cancellationToken);
                return new(EmailJobMessageResult.Completed);
            }

            var quotaResult = await quotaService.ReserveAsync(job.Id, job.AttemptCount, DateTime.UtcNow, cancellationToken);
            if (quotaResult == EmailQuotaReserveResult.LimitReached)
            {
                var retryAt = DateTime.UtcNow.Date.AddDays(1);
                job.Status = EmailJobStatus.DeferredQuota;
                job.NextAttemptAtUtc = retryAt;
                job.ProcessingLeaseUntilUtc = null;
                job.UpdatedAtUtc = DateTime.UtcNow;
                outreachRecipient.Status = "Queued";
                outreachRecipient.ProcessingAtUtc = null;
                outreachRecipient.ErrorMessage = "Daily outreach sending limit reached; queued for the next UTC day.";
                await db.SaveChangesAsync(cancellationToken);
                await RefreshCampaignForJobAsync(job, cancellationToken);
                return new(EmailJobMessageResult.Busy, retryAt);
            }
        }
        string providerMessageId;
        try
        {
            providerMessageId = await emailJobSender.SendAsync(
                job.Id, job.RecipientEmail, payload.ToName, payload.Subject, payload.HtmlBody,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Delivery outcome may be unknown. Do not blindly retry.
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Email job {EmailJobId} failed during provider delivery.",
                job.Id);

            job.Status = EmailJobStatus.OutcomeUnknown;
            job.ProcessingLeaseUntilUtc = null;
            job.UpdatedAtUtc = DateTime.UtcNow;
            job.LastErrorCode = "PROVIDER_OUTCOME_UNKNOWN";
            job.LastErrorMessage =
                "Delivery outcome could not be confirmed; check the provider log before retrying.";

            await MarkOutreachFailedAsync(
                job,
                job.LastErrorMessage,
                cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
            await RefreshCampaignForJobAsync(job, cancellationToken);

            return new(EmailJobMessageResult.Completed);
        }

        // Persist provider acceptance outside the SMTP exception handler.
        await MarkAcceptedAsync(job, providerMessageId, cancellationToken);
        if (job.LeadOutreachRecipientId.HasValue)
            await quotaService.ConsumeAsync(job.Id, job.AttemptCount, DateTime.UtcNow, cancellationToken);

        return new(EmailJobMessageResult.Completed);
    }

    private async Task MarkAcceptedAsync(
        EmailJob job,
        string providerMessageId,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        job.Status = EmailJobStatus.ProviderAccepted;
        job.DeliveryStatus = EmailDeliveryStatus.Accepted;
        job.ProviderMessageId = string.IsNullOrWhiteSpace(providerMessageId)
            ? null
            : providerMessageId[..Math.Min(providerMessageId.Length, 200)];
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
                .FirstOrDefaultAsync(
                    row => row.Id == job.LeadOutreachRecipientId.Value,
                    ct);

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
                    Details =
                        $"Campaign {recipient.CampaignId} ({recipient.Campaign.Subject}) accepted by the email provider.",
                    Actor = recipient.Campaign.CreatedBy
                });
            }
        }

        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task MarkSuppressedAsync(
        EmailJob job,
        LeadOutreachRecipient? recipient,
        CancellationToken ct)
    {
        job.Status = EmailJobStatus.Suppressed;
        job.ProcessingLeaseUntilUtc = null;
        job.UpdatedAtUtc = DateTime.UtcNow;

        if (recipient is not null)
        {
            recipient.Status = "Skipped";
            recipient.ProcessingAtUtc = null;
            recipient.ErrorMessage =
                "Lead is no longer eligible for email outreach.";
        }

        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task MarkOutreachFailedAsync(
        EmailJob job,
        string message,
        CancellationToken ct)
    {
        if (!job.LeadOutreachRecipientId.HasValue)
            return;

        var recipient = await db.LeadOutreachRecipients
            .FirstOrDefaultAsync(
                row => row.Id == job.LeadOutreachRecipientId.Value,
                ct);

        if (recipient is null)
            return;

        recipient.Status = "Failed";
        recipient.ProcessingAtUtc = null;
        recipient.ErrorMessage = message;
    }

    private async Task MarkPermanentFailureAsync(
        EmailJob job,
        string code,
        CancellationToken ct)
    {
        job.Status = EmailJobStatus.FailedPermanent;
        job.ProcessingLeaseUntilUtc = null;
        job.UpdatedAtUtc = DateTime.UtcNow;
        job.LastErrorCode = code;
        job.LastErrorMessage =
            "The email job payload is invalid and requires correction.";

        await MarkOutreachFailedAsync(
            job,
            job.LastErrorMessage,
            ct);

        await db.SaveChangesAsync(ct);
        await RefreshCampaignForJobAsync(job, ct);
    }

    private async Task RefreshCampaignForJobAsync(
        EmailJob job,
        CancellationToken ct)
    {
        if (!job.LeadOutreachRecipientId.HasValue)
            return;

        var campaignId = await db.LeadOutreachRecipients
            .Where(row => row.Id == job.LeadOutreachRecipientId.Value)
            .Select(row => (int?)row.CampaignId)
            .FirstOrDefaultAsync(ct);

        if (!campaignId.HasValue)
            return;

        var campaign = await db.LeadOutreachCampaigns
            .FirstOrDefaultAsync(
                row => row.Id == campaignId,
                ct);

        if (campaign is null)
            return;

        campaign.SentCount =
            await db.LeadOutreachRecipients.CountAsync(
                row => row.CampaignId == campaignId &&
                       row.Status == "Sent",
                ct);

        campaign.FailedCount =
            await db.LeadOutreachRecipients.CountAsync(
                row => row.CampaignId == campaignId &&
                       row.Status == "Failed",
                ct);

        var hasRemaining =
            await db.LeadOutreachRecipients.AnyAsync(
                row => row.CampaignId == campaignId &&
                       (row.Status == "Pending" ||
                        row.Status == "Processing" ||
                        row.Status == "Queued"),
                ct);

        campaign.Status = hasRemaining
            ? "Queued"
            : campaign.FailedCount > 0
                ? "CompletedWithErrors"
                : "Completed";

        campaign.CompletedAtUtc =
            hasRemaining ? null : DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsTerminal(EmailJobStatus status) =>
        status is
            EmailJobStatus.OutcomeUnknown or
            EmailJobStatus.ProviderAccepted or
            EmailJobStatus.FailedPermanent or
            EmailJobStatus.DeadLettered or
            EmailJobStatus.Suppressed or
            EmailJobStatus.Cancelled;
}
