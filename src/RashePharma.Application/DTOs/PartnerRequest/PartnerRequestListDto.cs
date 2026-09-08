namespace RashePharma.Application.DTOs.Partners;

public class PartnerRequestListDto
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? ExpectedVolume { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}