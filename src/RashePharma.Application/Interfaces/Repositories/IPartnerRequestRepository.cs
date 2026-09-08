using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IPartnerRequestRepository
{
    Task<List<PartnerRequest>> GetAllAsync();
    Task<PartnerRequest?> GetByIdAsync(int id);
    Task<List<PartnerRequest>> GetByUserIdAsync(int userId);
    Task AddAsync(PartnerRequest request);
    Task UpdateAsync(PartnerRequest request);
}