using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IPartnerRepository
{
    Task<List<Partner>> GetAllAsync();
    Task<Partner?> GetByIdAsync(int id);
    Task<Partner?> GetByUserIdAsync(int userId);
    Task AddAsync(Partner partner);
    Task UpdateAsync(Partner partner);
}