namespace RashePharma.Domain.Entities;

public class QuotationItem
{
    public int Id { get; set; }

    public int QuotationId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    // Navigation Properties
    public Quotation Quotation { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;
}