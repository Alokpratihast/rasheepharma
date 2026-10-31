
namespace RashePharma.Domain.Entities.Email;

public sealed class EmailQuotaReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmailJobId { get; set; }

    public Guid QuotaBucketId { get; set; }

    public int AttemptNumber { get; set; }

    public string Status { get; set; } = "Reserved";

    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ConsumedAtUtc { get; set; }

    public DateTime? ReleasedAtUtc { get; set; }

    public EmailJob EmailJob { get; set; } = null!;

    public EmailQuotaBucket QuotaBucket { get; set; } = null!;
}
