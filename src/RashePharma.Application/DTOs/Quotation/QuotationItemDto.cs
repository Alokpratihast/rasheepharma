namespace RashePharma.Application.DTOs.Quotations;

public class QuotationItemDto
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }
}