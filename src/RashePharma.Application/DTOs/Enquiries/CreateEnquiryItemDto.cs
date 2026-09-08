namespace RashePharma.Application.DTOs.Enquiries;

public class CreateEnquiryItemDto
{
    public int ProductVariantId { get; set; }

    public int Quantity { get; set; }

    public string? Message { get; set; }
}