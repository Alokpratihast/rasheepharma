namespace RashePharma.Domain.Entities;

public class Quotation
{
    public int Id { get; set; }

    public int EnquiryId { get; set; }

    public int? UserId { get; set; }

    public string QuoteNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = "Draft";

    public DateTime ValidUntil { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Enquiry Enquiry { get; set; } = null!;

    public User? User { get; set; }

    public ICollection<QuotationItem> Items { get; set; }
        = new List<QuotationItem>();
}