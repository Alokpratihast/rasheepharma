using RashePharma.Application.DTOs.Products;

namespace RashePharma.Application.Interfaces.Services;

public interface IProductVariantService
{
    Task<List<ProductVariantDto>> GetByProductIdAsync(int productId);

    Task<ProductVariantDto?> GetByIdAsync(int id);

    Task<ProductVariantDto> CreateAsync(
        int productId,
        ProductVariantCreateDto dto);

    Task<ProductVariantDto?> UpdateAsync(
        int id,
        ProductVariantUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}