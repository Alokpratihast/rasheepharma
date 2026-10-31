
namespace RashePharma.Domain.Entities.Email;

public sealed class EmailSuppression
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EmailNormalized { get; set; } = string.Empty;

    public string SuppressionType { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LiftedAtUtc { get; set; }

    public bool IsActive => LiftedAtUtc == null;
}
