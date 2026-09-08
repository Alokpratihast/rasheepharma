namespace RashePharma.Application.DTOs.Quotations;

public class QuotationListDto
{
    public int Id { get; set; }

    public string QuoteNumber { get; set; } = string.Empty;

    public int EnquiryId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public string Status { get; set; } = string.Empty;

    public DateTime ValidUntil { get; set; }

    public DateTime CreatedAt { get; set; }
}