namespace RashePharma.Application.DTOs.Products;

public class ProductUpdateDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? Composition { get; set; }

    public string? DosageForm { get; set; }

    public string? Description { get; set; }

    public string? Manufacturer { get; set; }

    public int CategoryId { get; set; }

    public bool IsActive { get; set; }
}