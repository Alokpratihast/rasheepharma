namespace RashePharma.Application.DTOs.Products;

public class ProductListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? GenericName { get; set; }

    public string? DosageForm { get; set; }

    public string? Manufacturer { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal? StartingPrice { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public bool IsActive { get; set; }
}