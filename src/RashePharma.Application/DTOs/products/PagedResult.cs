namespace RashePharma.Application.DTOs.Products;

/// <summary>
/// Standard metadata envelope for server-side paginated API responses.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages { get; init; }

    public bool HasNextPage { get; init; }

    public bool HasPreviousPage { get; init; }

    public IReadOnlyList<ProductFacetDto> CategoryFacets { get; init; } =
        Array.Empty<ProductFacetDto>();

    public IReadOnlyList<ProductFacetDto> FormFacets { get; init; } =
        Array.Empty<ProductFacetDto>();

    public int CategoryFacetTotalCount { get; init; }

    public int FormFacetTotalCount { get; init; }
}
