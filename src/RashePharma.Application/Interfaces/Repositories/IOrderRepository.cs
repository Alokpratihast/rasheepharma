using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<List<Order>> GetByUserIdAsync(int userId);

    Task<Order?> GetByIdAsync(int id);

    Task<Order?> GetByOrderNumberAsync(string orderNumber);

    Task AddAsync(Order order);

    Task UpdateAsync(Order order);

    Task AddStatusHistoryAsync(OrderStatusHistory history);

    Task<int> GetTotalCountAsync();
}