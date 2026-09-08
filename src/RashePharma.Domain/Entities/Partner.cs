namespace RashePharma.Domain.Entities;

public class Partner
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? PartnerRequestId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string Status { get; set; } = "Active";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User? User { get; set; }

    public PartnerRequest? PartnerRequest { get; set; }
}