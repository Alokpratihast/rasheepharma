using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByEmailAsync(string email);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task<bool> ExistsByEmailAsync(string email);

    Task<int> GetTotalCountAsync();

    Task<int> GetActiveCountAsync();
}