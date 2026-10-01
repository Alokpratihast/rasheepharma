namespace RashePharma.Domain.Entities;

public class EmailNotification
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public int AttemptCount { get; set; } = 0;

    public DateTime? LastAttemptAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Order Order { get; set; } = null!;
}