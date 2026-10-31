
namespace RashePharma.Domain.Enums.Email;

public enum EmailJobStatus
{
    Pending = 0,
    Queued = 1,
    Processing = 2,
    DeferredQuota = 3,
    RetryScheduled = 4,
    OutcomeUnknown = 5,
    ProviderAccepted = 6,
    FailedPermanent = 7,
    DeadLettered = 8,
    Suppressed = 9,
    Cancelled = 10
}
