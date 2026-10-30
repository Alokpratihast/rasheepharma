
namespace RashePharma.Domain.Entities.Email;

public sealed class EmailDeliveryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmailJobId { get; set; }

    public string Provider { get; set; } = "Brevo";

    public string? ProviderEventId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;

    public string? RawPayloadJson { get; set; }

    public EmailJob EmailJob { get; set; } = null!;
}
