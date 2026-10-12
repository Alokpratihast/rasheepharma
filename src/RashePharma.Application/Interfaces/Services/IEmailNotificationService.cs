namespace RashePharma.Application.Interfaces.Services;

public interface IEmailNotificationService
{
    Task ProcessPendingNotificationsAsync(
        int batchSize);
}