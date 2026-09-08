namespace RashePharma.Application.DTOs.Orders;

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;

    public string? Comment { get; set; }
}