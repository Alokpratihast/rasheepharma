namespace RashePharma.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductVariantId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    // Navigation Properties
    public Order Order { get; set; } = null!;

    public ProductVariant ProductVariant { get; set; } = null!;
}