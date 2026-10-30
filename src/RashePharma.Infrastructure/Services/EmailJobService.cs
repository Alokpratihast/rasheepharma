
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Infrastructure.Services;

public sealed class EmailJobService : IEmailJobService
{
    private const string IdempotencyIndexName =
        "IX_EmailJobs_IdempotencyKey";

    private readonly IEmailJobRepository _emailJobRepository;
    private readonly IEmailOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IServiceScopeFactory _scopeFactory;

    public EmailJobService(
        IEmailJobRepository emailJobRepository,
        IEmailOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        IServiceScopeFactory scopeFactory)
    {
        _emailJobRepository = emailJobRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _scopeFactory = scopeFactory;
    }

    public async Task<EmailJob> CreateJobAsync(
        EmailJob emailJob,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emailJob);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(emailJob.IdempotencyKey))
            throw new ArgumentException("Idempotency key is required.", nameof(emailJob));

        if (string.IsNullOrWhiteSpace(emailJob.RecipientEmail))
            throw new ArgumentException("Recipient email is required.", nameof(emailJob));

        var existingJob =
            await _emailJobRepository.GetByIdempotencyKeyAsync(
                emailJob.IdempotencyKey);

        if (existingJob is not null)
            return existingJob;

        try
        {
            await using (var transaction =
                await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    var outboxMessage = new EmailOutboxMessage
                    {
                        EmailJobId = emailJob.Id,
                        EventType = "EmailRequested",
                        SchemaVersion = 1,
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            EmailJobId = emailJob.Id,
                            emailJob.IdempotencyKey
                        }),
                        Status = "Pending",
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    await _emailJobRepository.AddAsync(emailJob);
                    await _outboxRepository.AddAsync(outboxMessage);

                    await _unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return emailJob;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        catch (DbUpdateException exception)
            when (IsIdempotencyConflict(exception))
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<IEmailJobRepository>();

            var concurrentlyCreatedJob =
                await repository.GetByIdempotencyKeyAsync(
                    emailJob.IdempotencyKey);

            if (concurrentlyCreatedJob is not null)
                return concurrentlyCreatedJob;

            throw;
        }
    }

    private static bool IsIdempotencyConflict(
        DbUpdateException exception)
    {
        var sqlException =
            exception.GetBaseException() as SqlException;

        if (sqlException is null)
            return false;

        var isDuplicateKey =
            sqlException.Number is 2601 or 2627;

        var isIdempotencyIndex =
            sqlException.Message.Contains(
                IdempotencyIndexName,
                StringComparison.OrdinalIgnoreCase);

        return isDuplicateKey && isIdempotencyIndex;
    }
}
