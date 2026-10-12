namespace RashePharma.Domain.Entities;

public class StripeWebhookEvent
{
    public int Id { get; set; }

    // Stripe's unique Event ID
    public string StripeEventId { get; set; } = string.Empty;

    // Example: checkout.session.completed
    public string EventType { get; set; } = string.Empty;

    // When Stripe created the event
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // When our system successfully processed the event
    public DateTime? ProcessedAt { get; set; }
}