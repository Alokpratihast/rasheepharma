namespace RashePharma.Application.DTOs.Products;

/// <summary>
/// A catalogue filter option and the number of active products matching it.
/// </summary>
public sealed class ProductFacetDto
{
    public string Name { get; init; } = string.Empty;

    public int Count { get; init; }
}
