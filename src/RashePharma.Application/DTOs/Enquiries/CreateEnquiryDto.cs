namespace RashePharma.Application.DTOs.Enquiries;

public class CreateEnquiryDto
{
    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? Message { get; set; }

    public List<CreateEnquiryItemDto> Items { get; set; } = new();
}