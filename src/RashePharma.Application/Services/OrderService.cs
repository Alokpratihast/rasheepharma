using RashePharma.Application.DTOs.Orders;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IAddressRepository addressRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _addressRepository = addressRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<OrderListDto>> GetMyOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders.Select(o => new OrderListDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        }).ToList();
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null || order.UserId != userId)
            return null;

        return MapToDetailsDto(order);
    }

    public async Task<OrderDetailsDto?> GetByOrderNumberAsync(
        string orderNumber,
        int userId)
    {
        var order = await _orderRepository
            .GetByOrderNumberAsync(orderNumber);

        if (order == null || order.UserId != userId)
            return null;

        return MapToDetailsDto(order);
    }

    public async Task<OrderDetailsDto> CreateAsync(
        int userId,
        CreateOrderDto dto)
    {
        // 1. Get user's cart
        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException(
                "Cart is empty.");
        }

        // 2. Validate shipping address
        var address = await _addressRepository
            .GetByIdAsync(dto.AddressId);

        if (address == null || address.UserId != userId)
        {
            throw new InvalidOperationException(
                "Invalid shipping address.");
        }

        // 3. Validate all cart items before creating order
        foreach (var cartItem in cart.Items)
        {
            var variant = await _productRepository
                .GetVariantByIdAsync(cartItem.ProductVariantId);

            if (variant == null)
            {
                throw new InvalidOperationException(
                    $"Product variant {cartItem.ProductVariantId} not found.");
            }

            if (!variant.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product variant {cartItem.ProductVariantId} is not active.");
            }

            if (variant.StockQuantity < cartItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for {variant.Product.Name}.");
            }
        }

        // 4. Begin transaction
        await using var transaction =
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                Currency = "INR",
                Status = "Pending",

                ShippingAddressLine1 = address.AddressLine1,
                ShippingAddressLine2 = address.AddressLine2,
                ShippingCity = address.City,
                ShippingState = address.State,
                ShippingPostalCode = address.PostalCode,
                ShippingCountry = address.Country
            };

            // 5. Create order items using latest DB price
            foreach (var cartItem in cart.Items)
            {
                var variant = await _productRepository
                    .GetVariantByIdAsync(cartItem.ProductVariantId);

                if (variant == null)
                {
                    throw new InvalidOperationException(
                        "Product variant not found.");
                }

                var unitPrice = variant.Price;

                var orderItem = new OrderItem
                {
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product.Name,
                    Strength = variant.Strength,
                    PackSize = variant.PackSize,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice
                };

                order.Items.Add(orderItem);

                order.TotalAmount +=
                    unitPrice * cartItem.Quantity;
            }

            // 6. Add order
            await _orderRepository.AddAsync(order);

            // 7. Add initial Pending status history
            var history = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "Pending",
                Comment = "Order created."
            };

            // Order ID is generated after SaveChanges,
            // so save the order first.
            await _unitOfWork.SaveChangesAsync();

            history.OrderId = order.Id;

            await _orderRepository
                .AddStatusHistoryAsync(history);

            // 8. Clear cart
            await _cartRepository
                .ClearItemsAsync(cart.Id);

            // 9. Save status history + cart changes
            await _unitOfWork.SaveChangesAsync();

            // 10. Commit transaction
            await transaction.CommitAsync();

            return MapToDetailsDto(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        UpdateOrderStatusDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(id);

        if (order == null)
            return false;

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        var history = new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = dto.Status,
            Comment = dto.Comment
        };

        await _orderRepository.AddStatusHistoryAsync(history);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static OrderDetailsDto MapToDetailsDto(Order order)
    {
        return new OrderDetailsDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Status = order.Status,

            ShippingAddressLine1 = order.ShippingAddressLine1,
            ShippingAddressLine2 = order.ShippingAddressLine2,
            ShippingCity = order.ShippingCity,
            ShippingState = order.ShippingState,
            ShippingPostalCode = order.ShippingPostalCode,
            ShippingCountry = order.ShippingCountry,

            CreatedAt = order.CreatedAt,

            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductName,
                Strength = item.Strength,
                PackSize = item.PackSize,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            }).ToList(),

            StatusHistory = order.StatusHistory
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => new OrderStatusHistoryDto
                {
                    Id = h.Id,
                    Status = h.Status,
                    Comment = h.Comment,
                    CreatedAt = h.CreatedAt
                }).ToList()
        };
    }
}