using RashePharma.Domain.Entities.BulkUpload;

namespace RashePharma.Application.Interfaces.Services;

public interface IBulkUploadDispatchPublisher
{
    Task PublishAsync(BulkUploadDispatchOutbox item, CancellationToken cancellationToken);
}
