
using RashePharma.Domain.Enums.Email;

namespace RashePharma.Domain.Entities.Email;

public sealed class EmailJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string IdempotencyKey { get; set; } = string.Empty;

    public EmailCategory Category { get; set; }

    public string EmailType { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;

    public string TemplateKey { get; set; } = string.Empty;

    public int TemplateVersion { get; set; } = 1;

    public string PayloadJson { get; set; } = "{}";

    public EmailJobStatus Status { get; set; } = EmailJobStatus.Pending;

    public int AttemptCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public string? ProviderMessageId { get; set; }

    public string CorrelationId { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessingLeaseUntilUtc { get; set; }

    public DateTime? AcceptedAtUtc { get; set; }

    public string? LastErrorCode { get; set; }

    public string? LastErrorMessage { get; set; }

    public EmailDeliveryStatus DeliveryStatus { get; set; }
        = EmailDeliveryStatus.Unknown;

    public byte[] RowVersion { get; set; } = [];
}
