namespace RashePharma.Application.DTOs.Products;

public class ProductVariantDto
{
    public int Id { get; set; }

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public decimal Price { get; set; }

    public string? SKU { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; }
}