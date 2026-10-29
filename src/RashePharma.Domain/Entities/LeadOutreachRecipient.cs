namespace RashePharma.Domain.Entities;

public sealed class LeadOutreachRecipient
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int LeadId { get; set; }
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessingAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public LeadOutreachCampaign Campaign { get; set; } = null!;
    public ExternalLead Lead { get; set; } = null!;
}
