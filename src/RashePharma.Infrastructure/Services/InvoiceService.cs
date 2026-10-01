using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public InvoiceService(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<byte[]> GenerateInvoiceAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new InvalidOperationException(
                "Order not found.");
        }

        var user = await _userRepository.GetByIdAsync(order.UserId);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Customer associated with the order was not found.");
        }

        if (order.Items == null || !order.Items.Any())
        {
            throw new InvalidOperationException(
                "Order has no items.");
        }

        if (!string.Equals(
                order.Currency,
                "USD",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invoice currency must be USD.");
        }

        var customerName =
            $"{user.FirstName} {user.LastName}".Trim();

        var invoiceDate =
            order.CreatedAt.ToString("dd MMM yyyy");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x =>
                    x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Spacing(5);

                        column.Item()
                            .Text("RASHE PHARMA")
                            .FontSize(24)
                            .Bold();

                        column.Item()
                            .Text("INVOICE")
                            .FontSize(18)
                            .Bold();

                        column.Item()
                            .LineHorizontal(1);
                    });

                page.Content()
                    .PaddingTop(20)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        column.Item()
                            .Row(row =>
                            {
                                row.RelativeItem()
                                    .Column(left =>
                                    {
                                        left.Item()
                                            .Text("Bill To")
                                            .Bold();

                                        left.Item()
                                            .Text(customerName);

                                        left.Item()
                                            .Text(user.Email);

                                        if (!string.IsNullOrWhiteSpace(
                                                user.PhoneNumber))
                                        {
                                            left.Item()
                                                .Text(
                                                    user.PhoneNumber);
                                        }
                                    });

                                row.RelativeItem()
                                    .Column(right =>
                                    {
                                        right.Item()
                                            .Text(
                                                $"Invoice No: {order.OrderNumber}");

                                        right.Item()
                                            .Text(
                                                $"Invoice Date: {invoiceDate}");

                                        right.Item()
                                            .Text(
                                                $"Order Status: {order.Status}");
                                    });
                            });

                        column.Item()
                            .Column(address =>
                            {
                                address.Item()
                                    .Text("Shipping Address")
                                    .Bold();

                                address.Item()
                                    .Text(
                                        order.ShippingAddressLine1);

                                if (!string.IsNullOrWhiteSpace(
                                        order.ShippingAddressLine2))
                                {
                                    address.Item()
                                        .Text(
                                            order.ShippingAddressLine2);
                                }

                                address.Item()
                                    .Text(
                                        $"{order.ShippingCity}, " +
                                        $"{order.ShippingState}");

                                address.Item()
                                    .Text(
                                        $"{order.ShippingPostalCode}, " +
                                        $"{order.ShippingCountry}");
                            });

                        column.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.ConstantColumn(45);
                                    columns.ConstantColumn(80);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell)
                                        .Text("Product");

                                    header.Cell().Element(HeaderCell)
                                        .Text("Strength");

                                    header.Cell().Element(HeaderCell)
                                        .Text("Pack");

                                    header.Cell().Element(HeaderCell)
                                        .AlignRight()
                                        .Text("Qty");

                                    header.Cell().Element(HeaderCell)
                                        .AlignRight()
                                        .Text("Unit Price");

                                    header.Cell().Element(HeaderCell)
                                        .AlignRight()
                                        .Text("Total");
                                });

                                foreach (var item in order.Items)
                                {
                                    var lineTotal =
                                        item.Quantity *
                                        item.UnitPrice;

                                    table.Cell().Element(BodyCell)
                                        .Text(item.ProductName);

                                    table.Cell().Element(BodyCell)
                                        .Text(item.Strength ?? "-");

                                    table.Cell().Element(BodyCell)
                                        .Text(item.PackSize ?? "-");

                                    table.Cell().Element(BodyCell)
                                        .AlignRight()
                                        .Text(
                                            item.Quantity.ToString());

                                    table.Cell().Element(BodyCell)
                                        .AlignRight()
                                        .Text(
                                            $"${item.UnitPrice:N2}");

                                    table.Cell().Element(BodyCell)
                                        .AlignRight()
                                        .Text(
                                            $"${lineTotal:N2}");
                                }
                            });

                        column.Item()
                            .AlignRight()
                            .Column(summary =>
                            {
                                summary.Item()
                                    .Text(
                                        $"Total Amount: ${order.TotalAmount:N2}")
                                    .FontSize(14)
                                    .Bold();

                                summary.Item()
                                    .Text("Currency: USD");

                                summary.Item()
                                    .Text("Payment Status: PAID")
                                    .Bold();
                            });

                        column.Item()
                            .PaddingTop(20)
                            .Text(
                                "Thank you for your business.")
                            .Italic();
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(
                        "Rashe Pharma • This is a computer-generated invoice.");
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .Padding(5)
            .BorderBottom(1);
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Padding(5)
            .BorderBottom(1);
    }
}