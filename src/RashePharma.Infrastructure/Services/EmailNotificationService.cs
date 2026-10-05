using System.Globalization;
using System.Net;

using Microsoft.Extensions.Logging;

using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private const int MaxAttempts = 5;

    private const string OrderInvoiceType = "OrderInvoice";
    private const string QuotationType = "Quotation";

    private readonly IEmailNotificationRepository _notificationRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IInvoiceService _invoiceService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IEmailNotificationRepository notificationRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IQuotationRepository quotationRepository,
        IInvoiceService invoiceService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<EmailNotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _quotationRepository = quotationRepository;
        _invoiceService = invoiceService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ProcessPendingNotificationsAsync(
        int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be greater than zero.");
        }

        var notifications =
            await _notificationRepository
                .ClaimPendingAsync(batchSize);

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
        EmailNotification notification)
    {
        try
        {
            if (notification.AttemptCount > MaxAttempts)
            {
                _logger.LogWarning(
                    "Skipping email notification {NotificationId} because maximum attempts were exceeded. Type: {Type}, OrderId: {OrderId}, QuotationId: {QuotationId}",
                    notification.Id,
                    notification.Type,
                    notification.OrderId,
                    notification.QuotationId);

                return;
            }

            if (string.Equals(
                    notification.Type,
                    OrderInvoiceType,
                    StringComparison.OrdinalIgnoreCase))
            {
                await ProcessOrderInvoiceAsync(notification);
                return;
            }

            if (string.Equals(
                    notification.Type,
                    QuotationType,
                    StringComparison.OrdinalIgnoreCase))
            {
                await ProcessQuotationAsync(notification);
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported email notification type: {notification.Type}");
        }
        catch (Exception ex)
        {
            notification.Status = "Failed";
            notification.ErrorMessage = ex.Message;

            await _notificationRepository
                .UpdateAsync(notification);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogError(
                ex,
                "Failed to process email notification. NotificationId: {NotificationId}, Type: {Type}, OrderId: {OrderId}, QuotationId: {QuotationId}, Attempt: {Attempt}",
                notification.Id,
                notification.Type,
                notification.OrderId,
                notification.QuotationId,
                notification.AttemptCount);
        }
    }

    private async Task ProcessOrderInvoiceAsync(
        EmailNotification notification)
    {
        if (!notification.OrderId.HasValue)
        {
            throw new InvalidOperationException(
                $"OrderId is required for notification {notification.Id}.");
        }

        var orderId = notification.OrderId.Value;

        var order =
            await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new InvalidOperationException(
                $"Order {orderId} was not found.");
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

        customerName =
            WebUtility.HtmlEncode(customerName);

        var orderNumber =
            WebUtility.HtmlEncode(order.OrderNumber);

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
                <strong>Order Number:</strong> {orderNumber}<br />
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

        await _notificationRepository
            .UpdateAsync(notification);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Invoice email sent successfully. NotificationId: {NotificationId}, OrderId: {OrderId}, OrderNumber: {OrderNumber}",
            notification.Id,
            order.Id,
            order.OrderNumber);
    }

    private async Task ProcessQuotationAsync(
        EmailNotification notification)
    {
        if (!notification.QuotationId.HasValue)
        {
            throw new InvalidOperationException(
                $"QuotationId is required for notification {notification.Id}.");
        }

        var quotationId =
            notification.QuotationId.Value;

        var quotation =
            await _quotationRepository
                .GetByIdAsync(quotationId);

        if (quotation == null)
        {
            throw new InvalidOperationException(
                $"Quotation {quotationId} was not found.");
        }

        if (quotation.Enquiry == null)
        {
            throw new InvalidOperationException(
                $"Enquiry for quotation {quotation.QuoteNumber} was not found.");
        }

        var customerEmail =
            quotation.Enquiry.Email?.Trim();

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new InvalidOperationException(
                $"Customer email is missing for quotation {quotation.QuoteNumber}.");
        }

        var customerName =
            quotation.Enquiry.CustomerName?.Trim();

        if (string.IsNullOrWhiteSpace(customerName))
        {
            customerName = "Customer";
        }

        customerName =
            WebUtility.HtmlEncode(customerName);

        var quoteNumber =
            WebUtility.HtmlEncode(
                quotation.QuoteNumber);

        var currency =
            WebUtility.HtmlEncode(
                quotation.Currency);

        var formattedTotalAmount =
            quotation.TotalAmount.ToString(
                "N2",
                CultureInfo.GetCultureInfo("en-US"));

        var itemDetails = string.Join(
            "",
            quotation.Items.Select(item =>
            {
                var productName =
                    WebUtility.HtmlEncode(
                        item.ProductVariant.Product.Name);

                var strength =
                    WebUtility.HtmlEncode(
                        item.ProductVariant.Strength ?? "-");

                var packSize =
                    WebUtility.HtmlEncode(
                        item.ProductVariant.PackSize ?? "-");

                var unitPrice =
                    item.UnitPrice.ToString(
                        "N2",
                        CultureInfo.GetCultureInfo("en-US"));

                var totalPrice =
                    item.TotalPrice.ToString(
                        "N2",
                        CultureInfo.GetCultureInfo("en-US"));

                return $"""
                    <div style="margin-bottom: 20px; padding: 16px; border: 1px solid #e5e7eb; border-radius: 8px;">

                        <p style="margin: 0 0 8px 0;">
                            <strong>Product:</strong>
                            {productName}
                        </p>

                        <p style="margin: 0 0 8px 0;">
                            <strong>Strength / Pack:</strong>
                            {strength} - {packSize}
                        </p>

                        <p style="margin: 0 0 8px 0;">
                            <strong>Quantity:</strong>
                            {item.Quantity:N0}
                        </p>

                        <p style="margin: 0 0 8px 0;">
                            <strong>Unit Price:</strong>
                            {currency} {unitPrice}
                        </p>

                        <p style="margin: 0;">
                            <strong>Total:</strong>
                            {currency} {totalPrice}
                        </p>

                    </div>
                    """;
            }));

        var notes =
            string.IsNullOrWhiteSpace(quotation.Notes)
                ? string.Empty
                : $"""
                    <p>
                        <strong>Notes:</strong><br />
                        {WebUtility.HtmlEncode(quotation.Notes)}
                    </p>
                    """;

        var subject =
            $"Rashe Pharma - Quotation {quotation.QuoteNumber}";

        var htmlBody = $"""
            <p>Dear {customerName},</p>

            <p>
                Thank you for your enquiry with Rashe Pharma.
            </p>

            <p>
                Please find your quotation details below.
            </p>

            <p>
                <strong>Quotation Number:</strong> {quoteNumber}<br />
                <strong>Valid Until:</strong> {quotation.ValidUntil:dd MMM yyyy}<br />
                <strong>Currency:</strong> {currency}
            </p>

            <h3>
                Quotation Details
            </h3>

            {itemDetails}

            <p style="font-size: 16px;">
                <strong>
                    Total Amount:
                </strong>
                {currency} {formattedTotalAmount}
            </p>

            {notes}

            <p>
                Please contact us if you have any questions regarding
                this quotation.
            </p>

            <p>
                Regards,<br />
                Rashe Pharma
            </p>
            """;

        _logger.LogInformation(
            "Sending quotation email. NotificationId: {NotificationId}, QuotationId: {QuotationId}, QuoteNumber: {QuoteNumber}, Attempt: {Attempt}",
            notification.Id,
            quotation.Id,
            quotation.QuoteNumber,
            notification.AttemptCount);

        await _emailService.SendAsync(
            customerEmail,
            customerName,
            subject,
            htmlBody);

        notification.Status = "Sent";
        notification.SentAt = DateTime.UtcNow;
        notification.ErrorMessage = null;

        await _notificationRepository
            .UpdateAsync(notification);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Quotation email sent successfully. NotificationId: {NotificationId}, QuotationId: {QuotationId}, QuoteNumber: {QuoteNumber}",
            notification.Id,
            quotation.Id,
            quotation.QuoteNumber);
    }
}