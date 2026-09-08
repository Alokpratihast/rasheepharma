namespace RashePharma.Application.DTOs.Quotations;

public class QuotationDetailsDto
{
    public int Id { get; set; }

    public string QuoteNumber { get; set; } = string.Empty;

    public int EnquiryId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public DateTime ValidUntil { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<QuotationItemDto> Items { get; set; } = new();
}