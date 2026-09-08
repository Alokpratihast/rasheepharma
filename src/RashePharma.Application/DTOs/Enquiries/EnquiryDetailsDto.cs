namespace RashePharma.Application.DTOs.Enquiries;

public class EnquiryDetailsDto
{
    public int Id { get; set; }

    public string EnquiryNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? Message { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<EnquiryItemDto> Items { get; set; } = new();
}