
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Services;

public interface IEmailJobService
{
    Task<EmailJob> CreateJobAsync(
        EmailJob emailJob,
        CancellationToken cancellationToken = default);
}
