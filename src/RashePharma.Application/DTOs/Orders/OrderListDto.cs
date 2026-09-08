namespace RashePharma.Application.DTOs.Orders;

public class OrderListDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }
}