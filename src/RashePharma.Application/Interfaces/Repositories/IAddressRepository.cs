using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IAddressRepository
{
    Task<List<Address>> GetByUserIdAsync(int userId);

    Task<Address?> GetByIdAsync(int id);

    Task AddAsync(Address address);

    Task UpdateAsync(Address address);

    Task DeleteAsync(Address address);
}