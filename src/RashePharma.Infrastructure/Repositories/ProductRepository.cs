using Microsoft.EntityFrameworkCore;
using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .ToListAsync();
    }

    public async Task<List<Product>> GetFeaturedAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.IsFeatured)
            .ToListAsync();
    }

    public async Task<PagedResult<ProductListDto>> GetPagedAsync(
        ProductQueryParameters parameters)
    {
        var baseQuery = _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Category.IsActive);

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var search = parameters.Search.Trim();

            baseQuery = baseQuery.Where(p =>
                p.Name.Contains(search) ||
                (p.GenericName != null && p.GenericName.Contains(search)) ||
                (p.Composition != null && p.Composition.Contains(search)) ||
                p.Category.Name.Contains(search));
        }

        // Category counts respect search and dosage-form filters, but ignore
        // the currently selected category so the user can switch categories.
        var categoryFacetQuery = baseQuery;
        if (!string.IsNullOrWhiteSpace(parameters.Form))
        {
            var formForCategoryFacet = parameters.Form.Trim();
            categoryFacetQuery = categoryFacetQuery.Where(
                p => p.DosageForm == formForCategoryFacet);
        }

        var categoryFacets = await categoryFacetQuery
            .GroupBy(p => p.Category.Name)
            .Select(group => new ProductFacetDto
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderBy(facet => facet.Name)
            .ToListAsync();
        var categoryFacetTotalCount = categoryFacets.Sum(facet => facet.Count);

        // Dosage-form counts respect search and category filters, but ignore
        // the currently selected dosage form.
        var formFacetQuery = baseQuery;
        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var categoryForFormFacet = parameters.Category.Trim();
            formFacetQuery = formFacetQuery.Where(
                p => p.Category.Name == categoryForFormFacet);
        }

        var formFacetTotalCount = await formFacetQuery.CountAsync();
        var formFacets = await formFacetQuery
            .Where(p => p.DosageForm != null && p.DosageForm != "")
            .GroupBy(p => p.DosageForm!)
            .Select(group => new ProductFacetDto
            {
                Name = group.Key,
                Count = group.Count()
            })
            .OrderBy(facet => facet.Name)
            .ToListAsync();

        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(parameters.Category))
        {
            var category = parameters.Category.Trim();
            query = query.Where(p => p.Category.Name == category);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Form))
        {
            var form = parameters.Form.Trim();
            query = query.Where(p => p.DosageForm == form);
        }

        var totalCount = await query.CountAsync();

        var products = query.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            GenericName = p.GenericName,
            Composition = p.Composition,
            DosageForm = p.DosageForm,
            BrandName = p.BrandName,
            Manufacturer = p.Manufacturer,
            CategoryName = p.Category.Name,
            StartingPrice = p.Variants
                .Where(v => v.IsActive)
                .Select(v => (decimal?)v.Price)
                .Min(),
            PackSize = p.Variants
                .Where(v => v.IsActive)
                .OrderBy(v => v.Id)
                .Select(v => v.PackSize)
                .FirstOrDefault(),
            MOQ = p.Variants
                .Where(v => v.IsActive)
                .OrderBy(v => v.Id)
                .Select(v => (int?)v.MOQ)
                .FirstOrDefault(),
            PrimaryImageUrl = p.Images
                .Where(i => i.IsPrimary)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.Id)
                .Select(i =>
                    i.ImageUrl.Contains(".blob.core.windows.net/")
                        ? $"/api/ProductImages/file/{i.Id}"
                        : i.ImageUrl)
                .FirstOrDefault(),
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured
        });

        products = parameters.SortBy switch
        {
            "name" when parameters.SortOrder == "desc" =>
                products.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
            "name" =>
                products.OrderBy(p => p.Name).ThenBy(p => p.Id),
            "price" when parameters.SortOrder == "desc" =>
                products.OrderBy(p => p.StartingPrice == null)
                    .ThenByDescending(p => p.StartingPrice)
                    .ThenBy(p => p.Id),
            "price" =>
                products.OrderBy(p => p.StartingPrice == null)
                    .ThenBy(p => p.StartingPrice)
                    .ThenBy(p => p.Id),
            _ =>
                products.OrderByDescending(p => p.IsFeatured)
                    .ThenBy(p => p.Name)
                    .ThenBy(p => p.Id)
        };

        var pageItems = await products
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        return new PagedResult<ProductListDto>
        {
            Items = pageItems,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = parameters.Page < totalPages,
            HasPreviousPage = parameters.Page > 1 && totalPages > 0,
            CategoryFacets = categoryFacets,
            FormFacets = formFacets,
            CategoryFacetTotalCount = categoryFacetTotalCount,
            FormFacetTotalCount = formFacetTotalCount
        };
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsBySlugAsync(string slug)
    {
        return await _context.Products
            .AnyAsync(p => p.Slug == slug);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<int> GetFeaturedCountAsync()
    {
        return await _context.Products
            .CountAsync(p => p.IsActive && p.IsFeatured);
    }
}
