namespace RashePharma.Domain.Entities;

public sealed class LeadOutreachCampaign
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public int RecipientCount { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? QueuedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public ICollection<LeadOutreachRecipient> Recipients { get; set; } = new List<LeadOutreachRecipient>();
}
