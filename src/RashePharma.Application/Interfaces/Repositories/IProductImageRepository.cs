using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IProductImageRepository
{
    Task<List<ProductImage>> GetByProductIdAsync(int productId);

    Task<ProductImage?> GetByIdAsync(int id);

    Task AddAsync(ProductImage image);

    Task UpdateAsync(ProductImage image);

    Task DeleteAsync(ProductImage image);
}