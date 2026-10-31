namespace RashePharma.Domain.Entities;

public sealed class ExternalLead
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? EmailNormalized { get; set; }
    public string? Phone { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Country { get; set; }
    public string? BusinessType { get; set; }
    public string? ProductInterest { get; set; }
    public string Source { get; set; } = "Manual";
    public string PermissionStatus { get; set; } = "Unknown";
    public string? PermissionEvidence { get; set; }
    public string? HistoricalMailSentOn { get; set; }
    public bool IsSuppressed { get; set; }
    public DateTime? SuppressedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = [];
}
