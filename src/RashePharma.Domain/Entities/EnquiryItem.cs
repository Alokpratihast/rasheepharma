namespace RashePharma.Domain.Entities;

public class EnquiryItem
{
    public int Id { get; set; }

    public int EnquiryId { get; set; }

    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public string? Message { get; set; }

    // Navigation Properties
    public Enquiry Enquiry { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;
}