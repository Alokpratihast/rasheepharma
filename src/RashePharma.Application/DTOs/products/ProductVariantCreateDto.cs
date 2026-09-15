namespace RashePharma.Application.DTOs.Products;

public class ProductVariantCreateDto
{
    public int ProductId { get; set; }

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public decimal Price { get; set; }

    public string? Currency { get; set; }

    public int? MOQ { get; set; }

    public string? UnitType { get; set; }

    public string? SKU { get; set; }

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;
}