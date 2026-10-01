using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private const int MaxAttempts = 5;
    private const string OrderInvoiceType = "OrderInvoice";

    private readonly IEmailNotificationRepository _notificationRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IInvoiceService _invoiceService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailNotificationRepository notificationRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IInvoiceService invoiceService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<EmailNotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _invoiceService = invoiceService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessPendingNotificationsAsync(int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be greater than zero.");
        }

        var notifications =
            await _notificationRepository.ClaimPendingAsync(batchSize);

        if (notifications.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Claimed {NotificationCount} email notifications for processing.",
            notifications.Count);

        foreach (var notification in notifications)
        {
            await ProcessNotificationAsync(notification);
        }
    }

    private async Task ProcessNotificationAsync(
        Domain.Entities.EmailNotification notification)
    {
        try
        {
            if (notification.AttemptCount > MaxAttempts)
            {
                _logger.LogWarning(
                    "Skipping email notification {NotificationId} because maximum attempts were exceeded. OrderId: {OrderId}",
                    notification.Id,
                    notification.OrderId);

                return;
            }

            if (!string.Equals(
                    notification.Type,
                    OrderInvoiceType,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Unsupported email notification type: {notification.Type}");
            }

            var order =
                await _orderRepository.GetByIdAsync(notification.OrderId);

            if (order == null)
            {
                throw new InvalidOperationException(
                    $"Order {notification.OrderId} was not found.");
            }

            var user =
                await _userRepository.GetByIdAsync(order.UserId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    $"Customer for order {order.OrderNumber} was not found.");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new InvalidOperationException(
                    $"Customer email is missing for order {order.OrderNumber}.");
            }

            var invoicePdf =
                await _invoiceService.GenerateInvoiceAsync(order.Id);

            var customerName =
                $"{user.FirstName} {user.LastName}".Trim();

            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = "Customer";
            }

            var subject =
                $"Rashe Pharma - Invoice {order.OrderNumber}";

            var htmlBody = $"""
                <p>Dear {customerName},</p>

                <p>
                    Thank you for your order with Rashe Pharma.
                </p>

                <p>
                    Your payment has been successfully received.
                    Please find your invoice attached to this email.
                </p>

                <p>
                    <strong>Order Number:</strong> {order.OrderNumber}<br />
                    <strong>Amount:</strong> ${order.TotalAmount:N2}<br />
                    <strong>Currency:</strong> USD
                </p>

                <p>
                    Regards,<br />
                    Rashe Pharma
                </p>
                """;

            var fileName =
                $"Invoice-{order.OrderNumber}.pdf";

            _logger.LogInformation(
                "Sending invoice email. NotificationId: {NotificationId}, OrderId: {OrderId}, OrderNumber: {OrderNumber}, Attempt: {Attempt}",
                notification.Id,
                order.Id,
                order.OrderNumber,
                notification.AttemptCount);

            await _emailService.SendOrderInvoiceAsync(
                user.Email,
                customerName,
                subject,
                htmlBody,
                invoicePdf,
                fileName);

            notification.Status = "Sent";
            notification.SentAt = DateTime.UtcNow;
            notification.ErrorMessage = null;

            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Invoice email sent successfully. NotificationId: {NotificationId}, OrderId: {OrderId}, OrderNumber: {OrderNumber}",
                notification.Id,
                order.Id,
                order.OrderNumber);
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;

            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogError(
                ex,
                "Failed to send invoice email. NotificationId: {NotificationId}, OrderId: {OrderId}, Attempt: {Attempt}",
                notification.Id,
                notification.OrderId,
                notification.AttemptCount);
        }
    }
}