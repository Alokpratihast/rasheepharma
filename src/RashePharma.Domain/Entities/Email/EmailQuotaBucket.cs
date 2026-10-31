
namespace RashePharma.Domain.Entities.Email;

public sealed class EmailQuotaBucket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Provider { get; set; } = "Brevo";

    public DateTime WindowStartUtc { get; set; }

    public DateTime WindowEndUtc { get; set; }

    public int DailyLimit { get; set; }

    public int ReservedCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public byte[] RowVersion { get; set; } = [];
}
