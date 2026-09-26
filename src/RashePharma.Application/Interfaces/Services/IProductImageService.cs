
using RashePharma.Application.DTOs.Products;

namespace RashePharma.Application.Interfaces.Services;

public interface IProductImageService
{
    Task<List<ProductImageDto>> GetByProductIdAsync(int productId);

    Task<ProductImageDto?> GetByIdAsync(int id);

    Task<ProductImageDto> CreateAsync(
        int productId,
        ProductImageCreateDto dto);

    Task<ProductImageDto?> UpdateAsync(
        int id,
        ProductImageUpdateDto dto);

   Task<ProductImageDto> UploadAsync(
    int productId,
    Stream fileStream,
    string fileName,
    string? contentType,
    string? altText,
    bool isPrimary,
    int displayOrder);

    Task<bool> DeleteAsync(int id);
}