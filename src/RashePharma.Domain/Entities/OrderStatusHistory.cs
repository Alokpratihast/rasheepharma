namespace RashePharma.Domain.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Order Order { get; set; } = null!;
}