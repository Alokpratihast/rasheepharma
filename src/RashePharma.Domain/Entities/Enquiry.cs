namespace RashePharma.Domain.Entities;

public class Enquiry
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string EnquiryNumber { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string Country { get; set; } = string.Empty;

    public string? BusinessType { get; set; }

    public string? Message { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User? User { get; set; }

    public ICollection<EnquiryItem> Items { get; set; }
        = new List<EnquiryItem>();
}