namespace RashePharma.Application.DTOs.Partners;

public class PartnerDto
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }
}