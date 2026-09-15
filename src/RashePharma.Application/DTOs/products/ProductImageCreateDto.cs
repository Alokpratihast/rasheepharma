namespace RashePharma.Application.DTOs.Products;

public class ProductImageCreateDto
{
    public string ImageUrl { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; }
}