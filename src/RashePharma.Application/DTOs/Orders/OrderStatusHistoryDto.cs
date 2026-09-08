namespace RashePharma.Application.DTOs.Orders;

public class OrderStatusHistoryDto
{
    public int Id { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}