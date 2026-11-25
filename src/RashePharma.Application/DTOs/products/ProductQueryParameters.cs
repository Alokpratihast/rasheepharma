using System.ComponentModel.DataAnnotations;

namespace RashePharma.Application.DTOs.Products;

/// <summary>
/// Query options for the public product catalogue.
/// </summary>
public sealed class ProductQueryParameters
{
    [Range(1, 1000000)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 12;

    [StringLength(120)]
    public string? Search { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(100)]
    public string? Form { get; set; }

    /// <summary>
    /// Supported values: featured, name, price.
    /// </summary>
    [RegularExpression("^(featured|name|price)$", ErrorMessage = "sortBy must be featured, name, or price.")]
    public string SortBy { get; set; } = "featured";

    /// <summary>
    /// Supported values: asc, desc.
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "sortOrder must be asc or desc.")]
    public string SortOrder { get; set; } = "desc";
}
