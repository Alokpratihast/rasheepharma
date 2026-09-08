namespace RashePharma.Application.DTOs.Quotations;

public class CreateQuotationDto
{
    public int EnquiryId { get; set; }

    public string Currency { get; set; } = "USD";

    public DateTime ValidUntil { get; set; }

    public string? Notes { get; set; }

    public List<CreateQuotationItemDto> Items { get; set; } = new();
}