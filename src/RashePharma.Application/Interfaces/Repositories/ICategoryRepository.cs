using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();

    Task<List<Category>> GetNavigationAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<Category?> GetBySlugAsync(string slug);

    Task AddAsync(Category category);

    Task UpdateAsync(Category category);

    Task DeleteAsync(Category category);

    Task<bool> ExistsBySlugAsync(string slug);

    Task<int> GetTotalCountAsync();

    Task<bool> HasChildrenAsync(int categoryId);

    Task<bool> HasProductsAsync(int categoryId);
}