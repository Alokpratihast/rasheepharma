
using RashePharma.Domain.Enums.Email;

namespace RashePharma.Application.Interfaces.Services;

public interface IEmailMessagePublisher
{
    Task PublishAsync(
        EmailCategory category,
        Guid outboxMessageId,
        Guid emailJobId,
        string eventType,
        int schemaVersion,
        string payloadJson,
        CancellationToken cancellationToken = default);
}
