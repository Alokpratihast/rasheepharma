namespace RashePharma.Application.DTOs.Enquiries;

public class EnquiryListDto
{
    public int Id { get; set; }

    public string EnquiryNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}