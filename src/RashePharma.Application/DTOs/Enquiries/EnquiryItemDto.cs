namespace RashePharma.Application.DTOs.Enquiries;

public class EnquiryItemDto
{
    public int Id { get; set; }

    public int ProductVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public int Quantity { get; set; }

    public string? Message { get; set; }
}