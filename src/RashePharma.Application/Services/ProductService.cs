using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductListDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products.Select(p => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            GenericName = p.GenericName,
            DosageForm = p.DosageForm,
            BrandName = p.BrandName,
            Manufacturer = p.Manufacturer,
            CategoryName = p.Category.Name,

            StartingPrice = p.Variants
                .Where(v => v.IsActive)
                .Select(v => (decimal?)v.Price)
                .Min(),

            PrimaryImageUrl = p.Images
                .Where(i => i.IsPrimary)
                .Select(i => i.ImageUrl)
                .FirstOrDefault(),

            IsActive = p.IsActive
        }).ToList();
    }

    public async Task<ProductDetailsDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return null;

        return MapToDetailsDto(product);
    }

    public async Task<ProductDetailsDto?> GetBySlugAsync(string slug)
    {
        var product = await _productRepository.GetBySlugAsync(slug);

        if (product == null)
            return null;

        return MapToDetailsDto(product);
    }

    public async Task<ProductDetailsDto> CreateAsync(ProductCreateDto dto)
    {
        // Check duplicate slug
        if (await _productRepository.ExistsBySlugAsync(dto.Slug))
        {
            throw new InvalidOperationException(
                "A product with this slug already exists.");
        }

        // Check category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

        if (category == null)
        {
            throw new InvalidOperationException(
                "Category does not exist.");
        }

        var product = new Product
        {
            Name = dto.Name,
            Slug = dto.Slug,
            GenericName = dto.GenericName,
            Composition = dto.Composition,
            DosageForm = dto.DosageForm,
            Description = dto.Description,
            BrandName = dto.BrandName,
            Manufacturer = dto.Manufacturer,
            CategoryId = dto.CategoryId,
            IsActive = dto.IsActive
        };

        await _productRepository.AddAsync(product);

        await _unitOfWork.SaveChangesAsync();

        var createdProduct = await _productRepository.GetByIdAsync(product.Id);

        if (createdProduct == null)
        {
            throw new InvalidOperationException(
                "Product could not be loaded after creation.");
        }

        return MapToDetailsDto(createdProduct);
    }

    public async Task<ProductDetailsDto?> UpdateAsync(
        int id,
        ProductUpdateDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return null;

        if (product.Slug != dto.Slug &&
            await _productRepository.ExistsBySlugAsync(dto.Slug))
        {
            throw new InvalidOperationException(
                "A product with this slug already exists.");
        }

        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

        if (category == null)
        {
            throw new InvalidOperationException(
                "Category does not exist.");
        }

        product.Name = dto.Name;
        product.Slug = dto.Slug;
        product.GenericName = dto.GenericName;
        product.Composition = dto.Composition;
        product.DosageForm = dto.DosageForm;
        product.Description = dto.Description;
        product.BrandName = dto.BrandName;
        product.Manufacturer = dto.Manufacturer;
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return MapToDetailsDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return false;

        await _productRepository.DeleteAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static ProductDetailsDto MapToDetailsDto(Product product)
    {
        return new ProductDetailsDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            GenericName = product.GenericName,
            Composition = product.Composition,
            DosageForm = product.DosageForm,
            Description = product.Description,
            BrandName = product.BrandName,
            Manufacturer = product.Manufacturer,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            IsActive = product.IsActive,

            Variants = product.Variants
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Strength = v.Strength,
                    PackSize = v.PackSize,
                    Price = v.Price,
                    Currency = v.Currency,
                    MOQ = v.MOQ,
                    UnitType = v.UnitType,
                    SKU = v.SKU,
                    StockQuantity = v.StockQuantity,
                    IsActive = v.IsActive
                })
                .ToList(),

            Images = product.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => new ProductImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                })
                .ToList()
        };
    }
}