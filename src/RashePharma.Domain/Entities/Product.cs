namespace RashePharma.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? Composition { get; set; }

    public string? DosageForm { get; set; }

    public string? Description { get; set; }

    public string? BrandName { get; set; }

    public string? Manufacturer { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Property
    public Category Category { get; set; } = null!;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}