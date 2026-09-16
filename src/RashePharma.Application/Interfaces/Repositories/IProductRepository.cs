using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync();

    Task<List<Product>> GetFeaturedAsync();

    Task<Product?> GetByIdAsync(int id);

    Task<Product?> GetBySlugAsync(string slug);

    Task AddAsync(Product product);

    Task UpdateAsync(Product product);

    Task DeleteAsync(Product product);

    Task<bool> ExistsBySlugAsync(string slug);

    Task<int> GetTotalCountAsync();

    Task<int> GetFeaturedCountAsync();
}