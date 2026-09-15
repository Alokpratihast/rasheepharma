using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class ProductVariantService : IProductVariantService
{
    private readonly IProductVariantRepository _variantRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductVariantService(
        IProductVariantRepository variantRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _variantRepository = variantRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ProductVariantDto>> GetByProductIdAsync(
        int productId)
    {
        var variants = await _variantRepository
            .GetByProductIdAsync(productId);

        return variants.Select(MapToDto).ToList();
    }

    public async Task<ProductVariantDto?> GetByIdAsync(int id)
    {
        var variant = await _variantRepository.GetByIdAsync(id);

        if (variant == null)
            return null;

        return MapToDto(variant);
    }

    public async Task<ProductVariantDto> CreateAsync(
        int productId,
        ProductVariantCreateDto dto)
    {
        var product = await _productRepository.GetByIdAsync(productId);

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with ID {productId} was not found.");
        }

        var variant = new ProductVariant
        {
            ProductId = productId,
            Strength = dto.Strength,
            PackSize = dto.PackSize,
            Price = dto.Price,
            Currency = dto.Currency,
            MOQ = dto.MOQ,
            UnitType = dto.UnitType,
            SKU = dto.SKU,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive
        };

        await _variantRepository.AddAsync(variant);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(variant);
    }

    public async Task<ProductVariantDto?> UpdateAsync(
        int id,
        ProductVariantUpdateDto dto)
    {
        var variant = await _variantRepository.GetByIdAsync(id);

        if (variant == null)
            return null;

        variant.Strength = dto.Strength;
        variant.PackSize = dto.PackSize;
        variant.Price = dto.Price;
        variant.Currency = dto.Currency;
        variant.MOQ = dto.MOQ;
        variant.UnitType = dto.UnitType;
        variant.SKU = dto.SKU;
        variant.StockQuantity = dto.StockQuantity;
        variant.IsActive = dto.IsActive;
        variant.UpdatedAt = DateTime.UtcNow;

        await _variantRepository.UpdateAsync(variant);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(variant);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var variant = await _variantRepository.GetByIdAsync(id);

        if (variant == null)
            return false;

        await _variantRepository.DeleteAsync(variant);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static ProductVariantDto MapToDto(
        ProductVariant variant)
    {
        return new ProductVariantDto
        {
            Id = variant.Id,
            Strength = variant.Strength,
            PackSize = variant.PackSize,
            Price = variant.Price,
            Currency = variant.Currency,
            MOQ = variant.MOQ,
            UnitType = variant.UnitType,
            SKU = variant.SKU,
            StockQuantity = variant.StockQuantity,
            IsActive = variant.IsActive
        };
    }
}