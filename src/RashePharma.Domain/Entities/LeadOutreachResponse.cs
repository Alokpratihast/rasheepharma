namespace RashePharma.Domain.Entities;

/// <summary>A single, one-time reply submitted by a campaign recipient.</summary>
public sealed class LeadOutreachResponse
{
    public long Id { get; set; }
    public int RecipientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
    public LeadOutreachRecipient Recipient { get; set; } = null!;
}
