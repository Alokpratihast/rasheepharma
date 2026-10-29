namespace RashePharma.Domain.Entities;

public sealed class LeadActivity
{
    public long Id { get; set; }
    public int LeadId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
