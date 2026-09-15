namespace RashePharma.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string? Country { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    

    // Navigation Property
    public Role Role { get; set; } = null!;
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public Cart Cart { get; set; } = null!;

    public ICollection<Order> Orders { get; set; }
    = new List<Order>();
    public ICollection<Enquiry> Enquiries { get; set; }
    = new List<Enquiry>();
}