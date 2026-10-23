using System.ComponentModel.DataAnnotations;

namespace RashePharma.Domain.Entities;

public sealed class LocationSeoPage
{
    public int Id { get; set; }

    [MaxLength(100)] public string Country { get; set; } = string.Empty;
    [MaxLength(2)] public string CountryCode { get; set; } = string.Empty;
    [MaxLength(100)] public string CountrySlug { get; set; } = string.Empty;
    [MaxLength(3)] public string QuoteCurrency { get; set; } = "USD";
    [MaxLength(120)] public string City { get; set; } = string.Empty;
    [MaxLength(120)] public string Region { get; set; } = string.Empty;
    [MaxLength(120)] public string CitySlug { get; set; } = string.Empty;
    [MaxLength(200)] public string MetaTitle { get; set; } = string.Empty;
    [MaxLength(320)] public string MetaDescription { get; set; } = string.Empty;
    [MaxLength(200)] public string Heading { get; set; } = string.Empty;
    [MaxLength(3000)] public string Intro { get; set; } = string.Empty;
    [MaxLength(2000)] public string BuyerFocus { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public bool IsIndexable { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
