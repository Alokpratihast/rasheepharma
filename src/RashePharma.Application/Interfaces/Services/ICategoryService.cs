using RashePharma.Application.DTOs.Categories;

namespace RashePharma.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryListDto>> GetAllAsync();

    Task<List<CategoryNavigationDto>> GetNavigationAsync();

    Task<CategoryDetailsDto?> GetByIdAsync(int id);

    Task<CategoryDetailsDto?> GetBySlugAsync(string slug);

    Task<CategoryDetailsDto> CreateAsync(CategoryCreateDto dto);

    Task<CategoryDetailsDto?> UpdateAsync(
        int id,
        CategoryUpdateDto dto);

    Task<bool> DeleteAsync(int id);
}