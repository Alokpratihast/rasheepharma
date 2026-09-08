namespace RashePharma.Application.DTOs.Products;

public class ProductDetailsDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? Composition { get; set; }

    public string? DosageForm { get; set; }

    public string? Description { get; set; }

    public string? Manufacturer { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public List<ProductVariantDto> Variants { get; set; } = new();

    public List<ProductImageDto> Images { get; set; } = new();
}