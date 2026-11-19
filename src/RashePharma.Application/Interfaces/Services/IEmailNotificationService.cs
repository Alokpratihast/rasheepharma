namespace RashePharma.Application.Interfaces.Services;

public interface IEmailNotificationService
{
    Task<int> ProcessPendingNotificationsAsync(int batchSize);
}
