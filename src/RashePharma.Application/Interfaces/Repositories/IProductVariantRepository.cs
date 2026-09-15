using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IProductVariantRepository
{
    Task<List<ProductVariant>> GetByProductIdAsync(int productId);

    Task<ProductVariant?> GetByIdAsync(int id);

    Task AddAsync(ProductVariant variant);

    Task UpdateAsync(ProductVariant variant);

    Task DeleteAsync(ProductVariant variant);
}