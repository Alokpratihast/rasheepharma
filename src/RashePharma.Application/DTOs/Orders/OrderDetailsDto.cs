namespace RashePharma.Application.DTOs.Orders;

public class OrderDetailsDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Pending";

    // Customer Details
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string ShippingAddressLine1 { get; set; } = string.Empty;

    public string? ShippingAddressLine2 { get; set; }

    public string ShippingCity { get; set; } = string.Empty;

    public string? ShippingState { get; set; }

    public string ShippingPostalCode { get; set; } = string.Empty;

    public string ShippingCountry { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();

    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}