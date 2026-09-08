namespace RashePharma.Domain.Entities;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public decimal Price { get; set; }

    public string? SKU { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Property
    public Product Product { get; set; } = null!;
}