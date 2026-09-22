namespace RashePharma.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Pending";

    // Shipping Address Snapshot
    public string ShippingAddressLine1 { get; set; } = string.Empty;

    public string? ShippingAddressLine2 { get; set; }

    public string ShippingCity { get; set; } = string.Empty;

    public string? ShippingState { get; set; }

    public string ShippingPostalCode { get; set; } = string.Empty;

    public string ShippingCountry { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; }
        = new List<OrderStatusHistory>();

    public ICollection<Payment> Payments { get; set; }
    = new List<Payment>();
}