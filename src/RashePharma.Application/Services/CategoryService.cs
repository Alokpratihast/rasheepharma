using RashePharma.Application.DTOs.Categories;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryListDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(c => new CategoryListDto
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            IsActive = c.IsActive,

            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name

        }).ToList();
    }

    public async Task<List<CategoryNavigationDto>> GetNavigationAsync()
    {
        var categories = await _categoryRepository.GetNavigationAsync();

        var rootCategories = categories
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .ToList();

        return rootCategories
            .Select(MapToNavigationDto)
            .ToList();
    }

    public async Task<CategoryDetailsDto?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return null;

        return MapToDetailsDto(category);
    }

    public async Task<CategoryDetailsDto?> GetBySlugAsync(string slug)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug);

        if (category == null)
            return null;

        return MapToDetailsDto(category);
    }

    public async Task<CategoryDetailsDto> CreateAsync(CategoryCreateDto dto)
    {
        if (await _categoryRepository.ExistsBySlugAsync(dto.Slug))
            throw new InvalidOperationException(
                "A category with this slug already exists.");

        // Validate Parent Category
        if (dto.ParentCategoryId.HasValue)
        {
            var parentCategory =
                await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);

            if (parentCategory == null)
                throw new InvalidOperationException(
                    "The selected parent category does not exist.");
        }

        var category = new Category
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            IsActive = dto.IsActive,

            ParentCategoryId = dto.ParentCategoryId
        };

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToDetailsDto(category);
    }

    public async Task<CategoryDetailsDto?> UpdateAsync(
        int id,
        CategoryUpdateDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return null;

        if (category.Slug != dto.Slug &&
            await _categoryRepository.ExistsBySlugAsync(dto.Slug))
        {
            throw new InvalidOperationException(
                "A category with this slug already exists.");
        }

        // Validate Parent Category
        if (dto.ParentCategoryId.HasValue)
        {
            // Category cannot be its own parent
            if (dto.ParentCategoryId.Value == id)
                throw new InvalidOperationException(
                    "A category cannot be its own parent.");

            var parentCategory =
                await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);

            if (parentCategory == null)
                throw new InvalidOperationException(
                    "The selected parent category does not exist.");
        }

        category.Name = dto.Name;
        category.Slug = dto.Slug;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        category.ParentCategoryId = dto.ParentCategoryId;

        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToDetailsDto(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
            return false;

        await _categoryRepository.DeleteAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static CategoryNavigationDto MapToNavigationDto(Category category)
    {
        return new CategoryNavigationDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,

            Children = category.Children
                .Where(c => c.IsActive)
                .Select(MapToNavigationDto)
                .ToList(),

            Products = category.Products
                .Where(p => p.IsActive)
                .Select(p => new CategoryNavigationProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug
                })
                .ToList()
        };
    }

    private static CategoryDetailsDto MapToDetailsDto(Category category)
    {
        return new CategoryDetailsDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,

            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,

            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}