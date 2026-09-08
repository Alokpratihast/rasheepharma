namespace RashePharma.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; }

    // Navigation Property
    public Product Product { get; set; } = null!;
}