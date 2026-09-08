namespace RashePharma.Application.DTOs.Partners;

public class PartnerRequestCreateDto
{
    public string CompanyName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? ExpectedVolume { get; set; }

    public string? Message { get; set; }
}