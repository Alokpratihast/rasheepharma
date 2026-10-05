namespace RashePharma.Domain.Entities;

public class EmailNotification
{
    public int Id { get; set; }

    // Used for OrderInvoice notifications.
    // Null for quotation notifications.
    public int? OrderId { get; set; }

    // Used for Quotation notifications.
    // Null for OrderInvoice notifications.
    public int? QuotationId { get; set; }

    // Examples:
    // "OrderInvoice"
    // "Quotation"
    public string Type { get; set; } = string.Empty;

    // Pending -> Processing -> Sent
    // Failed notifications can be retried by the worker.
    public string Status { get; set; } = "Pending";

    public int AttemptCount { get; set; } = 0;

    public DateTime? LastAttemptAt { get; set; }

    public DateTime? SentAt { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    public Order? Order { get; set; }

    public Quotation? Quotation { get; set; }
}