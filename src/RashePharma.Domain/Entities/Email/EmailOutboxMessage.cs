
namespace RashePharma.Domain.Entities.Email;

public sealed class EmailOutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmailJobId { get; set; }

    public string EventType { get; set; } = "EmailRequested";

    public int SchemaVersion { get; set; } = 1;

    public string PayloadJson { get; set; } = "{}";

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAtUtc { get; set; }

    public int AttemptCount { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public DateTime? LeaseUntilUtc { get; set; }

    public string? LastError { get; set; }

    public byte[] RowVersion { get; set; } = [];
}
