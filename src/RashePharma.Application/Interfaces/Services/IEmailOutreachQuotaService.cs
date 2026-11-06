namespace RashePharma.Application.Interfaces.Services;

public enum EmailQuotaReserveResult
{
    Reserved,
    AlreadyReserved,
    LimitReached
}

public interface IEmailOutreachQuotaService
{
    Task<EmailQuotaReserveResult> ReserveAsync(
        Guid emailJobId,
        int attemptNumber,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task ConsumeAsync(
        Guid emailJobId,
        int attemptNumber,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        Guid emailJobId,
        int attemptNumber,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
