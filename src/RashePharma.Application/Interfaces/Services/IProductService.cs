using RashePharma.Application.DTOs.Products;

namespace RashePharma.Application.Interfaces.Services;

public interface IProductService
{
    Task<List<ProductListDto>> GetAllAsync();
    Task<ProductDetailsDto?> GetByIdAsync(int id);
    Task<ProductDetailsDto?> GetBySlugAsync(string slug);

    Task<ProductDetailsDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDetailsDto?> UpdateAsync(int id, ProductUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}