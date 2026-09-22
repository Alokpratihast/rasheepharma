namespace RashePharma.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    // Order
    public int OrderId { get; set; }

    // Payment Details
    public decimal Amount { get; set; }

    public string Currency { get; set; } = "INR";

    public string Status { get; set; } = "Pending";

    public string? PaymentMethod { get; set; }

    // Stripe Details
    public string? StripeSessionId { get; set; }

    public string? StripePaymentIntentId { get; set; }

    public string? StripeCustomerId { get; set; }

    // Failure Information
    public string? FailureReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation Property
    public Order Order { get; set; } = null!;
}