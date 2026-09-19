using RashePharma.Application.DTOs.Orders;

namespace RashePharma.Application.Interfaces.Services;

public interface IOrderService
{
    Task<List<OrderListDto>> GetMyOrdersAsync(int userId);

    // Admin - get all orders
    Task<List<OrderListDto>> GetAllOrdersAsync();

    Task<OrderDetailsDto?> GetAdminOrderByIdAsync(int id);

    Task<OrderDetailsDto?> GetByIdAsync(int id, int userId);
    Task<OrderDetailsDto?> GetByOrderNumberAsync(string orderNumber, int userId);
    Task<OrderDetailsDto> CreateAsync(int userId, CreateOrderDto dto);
    Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusDto dto);
}