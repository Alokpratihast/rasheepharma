namespace RashePharma.Application.DTOs.Orders;

public class OrderDetailsDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Pending";

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