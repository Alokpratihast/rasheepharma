namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadExcelParser
{
    Task<BulkUploadExcelData> ParseAsync(
        Stream excelStream,
        CancellationToken cancellationToken = default);
}

public class BulkUploadExcelData
{
    public List<BulkUploadProductRow> Products { get; set; } = new();

    public List<BulkUploadVariantRow> Variants { get; set; } = new();

    public List<BulkUploadImageRow> Images { get; set; } = new();
}

public class BulkUploadProductRow
{
    public string Category { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Composition { get; set; }
    public string? DosageForm { get; set; }
    public string? Description { get; set; }
    public string? BrandName { get; set; }
    public string? Manufacturer { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
}

public class BulkUploadVariantRow
{
    public string ProductSlug { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string? PackSize { get; set; }
    public decimal PriceInr { get; set; }
    public int MOQ { get; set; }
    public string? UnitType { get; set; }
    public string? SKU { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}

public class BulkUploadImageRow
{
    public string ProductSlug { get; set; } = string.Empty;
    public string ImageFileName { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}