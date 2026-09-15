namespace RashePharma.Application.DTOs.Products;

public class ProductImageUpdateDto
{
    public string ImageUrl { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }
}