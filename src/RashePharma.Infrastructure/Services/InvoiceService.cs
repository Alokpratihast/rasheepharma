using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private const string LogoResourceName = "RashePharma.Infrastructure.Assets.Rashelifescience.png";
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo DateCulture = CultureInfo.GetCultureInfo("en-GB");
    private static readonly byte[]? CompanyLogo = LoadCompanyLogo();

    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public InvoiceService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _configuration = configuration;
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

        var invoiceDate = order.CreatedAt.ToString("dd MMM yyyy", DateCulture);
        var companyName = _configuration["Company:Name"] ?? "Rashe Life Sciences Pvt. Ltd.";
        var companyPhone = _configuration["Company:Phone"] ?? "+91 91104 56656";
        var companyEmail = _configuration["Company:Email"] ?? "hr.rashelifescience@gmail.com";
        var companyWebsite = _configuration["Company:WebsiteUrl"] ?? "https://rashelifesciences.com";
        var companyAddress = _configuration["Company:Address"] ?? "01, B Byraveshwara Nagar, Magadi Road, Sunkadakatte, Bangalore, Karnataka 560091";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(42);
                page.MarginVertical(36);
                page.DefaultTextStyle(x =>
                    x.FontSize(9).FontColor("#243447"));

                page.Header()
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(190).Height(54).Element(container =>
                            {
                                if (CompanyLogo is { Length: > 0 })
                                {
                                    container.Image(CompanyLogo).FitArea();
                                }
                                else
                                {
                                    container.AlignMiddle().Text(companyName)
                                        .FontSize(16).Bold().FontColor("#073F34");
                                }
                            });

                            row.RelativeItem().AlignRight().Column(details =>
                            {
                                details.Item().AlignRight().Text("INVOICE")
                                    .FontSize(22).Bold().FontColor("#073F34");
                                details.Item().AlignRight().Text($"Invoice {order.OrderNumber}")
                                    .FontSize(9).FontColor("#64748B");
                            });
                        });

                        column.Item().PaddingTop(12).LineHorizontal(2).LineColor("#07865F");
                        column.Item().PaddingTop(5).Text("QUALITY HEALTHCARE · BUILT FOR TRUST")
                            .FontSize(8).SemiBold().FontColor("#07865F").LetterSpacing(1);
                    });

                page.Content()
                    .PaddingTop(20)
                    .Column(column =>
                    {
                        column.Spacing(18);

                        column.Item().PaddingBottom(2).Text("Payment received")
                            .FontSize(19).Bold().FontColor("#172B4D");

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(container => container
                                .Border(1).BorderColor("#E2EAE6").Background("#F6FAF8")
                                .Padding(14).Column(left =>
                                {
                                    left.Item().Text("BILL TO").FontSize(8).Bold().FontColor("#64748B").LetterSpacing(.7f);
                                    left.Item().PaddingTop(5).Text(customerName).FontSize(11).Bold().FontColor("#172B4D");
                                    left.Item().Text(user.Email).FontSize(9).FontColor("#52616B");
                                    if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
                                    {
                                        left.Item().Text(user.PhoneNumber).FontSize(9).FontColor("#52616B");
                                    }
                                }));

                            row.ConstantItem(12);

                            row.RelativeItem().Element(container => container
                                .Border(1).BorderColor("#E2EAE6").Background("#F6FAF8")
                                .Padding(14).Column(right =>
                                {
                                    right.Item().Text("INVOICE DETAILS").FontSize(8).Bold().FontColor("#64748B").LetterSpacing(.7f);
                                    right.Item().PaddingTop(5).Text($"Invoice date: {invoiceDate}").FontSize(9);
                                    right.Item().Text($"Order number: {order.OrderNumber}").FontSize(9);
                                    right.Item().Text($"Order status: {order.Status}").FontSize(9);
                                }));
                        });

                        column.Item().Element(container => container
                            .Border(1).BorderColor("#E2EAE6").Padding(14).Column(address =>
                            {
                                address.Item().Text("SHIP TO").FontSize(8).Bold().FontColor("#64748B").LetterSpacing(.7f);
                                address.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span(order.ShippingAddressLine1).SemiBold();
                                    if (!string.IsNullOrWhiteSpace(order.ShippingAddressLine2))
                                    {
                                        text.Line(order.ShippingAddressLine2);
                                    }
                                    text.Line(string.Join(", ", new[] { order.ShippingCity, order.ShippingState }
                                        .Where(value => !string.IsNullOrWhiteSpace(value))));
                                    text.Line(string.Join(" ", new[] { order.ShippingPostalCode, order.ShippingCountry }
                                        .Where(value => !string.IsNullOrWhiteSpace(value))));
                                });
                            }));

                        column.Item().Text("ITEMIZED ORDER").FontSize(11).Bold().FontColor("#172B4D");

                        column.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3.1f);
                                    columns.RelativeColumn(1.15f);
                                    columns.RelativeColumn(1.05f);
                                    columns.RelativeColumn(.6f);
                                    columns.RelativeColumn(1.25f);
                                    columns.RelativeColumn(1.25f);
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
                                            .Text("Amount");
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
                                            $"{order.Currency} {item.UnitPrice.ToString("N2", DisplayCulture)}");

                                    table.Cell().Element(BodyCell)
                                        .AlignRight()
                                        .Text(
                                            $"{order.Currency} {lineTotal.ToString("N2", DisplayCulture)}");
                                }
                            });

                        column.Item().AlignRight().Width(260).Element(container => container
                            .Background("#F2F8F5").Border(1).BorderColor("#DDEBE4")
                            .Padding(16).Column(summary =>
                            {
                                summary.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Payment status").FontSize(9).FontColor("#52616B");
                                    row.AutoItem().Text("PAID").FontSize(9).Bold().FontColor("#08775A");
                                });
                                summary.Item().PaddingTop(9).LineHorizontal(1).LineColor("#DDEBE4");
                                summary.Item().PaddingTop(9).Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL PAID").FontSize(9).Bold().FontColor("#52616B");
                                    row.AutoItem().Text($"{order.Currency} {order.TotalAmount.ToString("N2", DisplayCulture)}")
                                        .FontSize(16).Bold().FontColor("#073F34");
                                });
                            }));

                        column.Item().PaddingTop(8).Column(signature =>
                        {
                            signature.Item().Text("Thank you for your business.")
                                .FontSize(10).Italic().FontColor("#52616B");
                            signature.Item().PaddingTop(14).Row(row =>
                            {
                                if (CompanyLogo is { Length: > 0 })
                                {
                                    row.ConstantItem(150).Height(44).Image(CompanyLogo).FitArea();
                                }
                                else
                                {
                                    row.ConstantItem(150).Height(44).AlignMiddle().Text(companyName)
                                        .FontSize(12).Bold().FontColor("#073F34");
                                }
                                row.RelativeItem().AlignRight().AlignMiddle().Text($"For {companyName}\nCompany acknowledgement")
                                    .FontSize(8).FontColor("#64748B");
                            });
                        });
                    });

                page.Footer()
                    .PaddingTop(10)
                    .BorderTop(1).BorderColor("#E2EAE6")
                    .Row(row =>
                    {
                        row.RelativeItem().Column(contact =>
                        {
                            contact.Item().Text(companyName).FontSize(8).Bold().FontColor("#334155");
                            contact.Item().Text($"{companyPhone}  ·  {companyEmail}").FontSize(7).FontColor("#64748B");
                            contact.Item().Text(companyAddress).FontSize(7).FontColor("#64748B");
                            contact.Item().Text(companyWebsite).FontSize(7).FontColor("#08775A");
                        });
                        row.AutoItem().AlignBottom().Text("Computer-generated invoice")
                            .FontSize(7).FontColor("#84918D");
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Background("#073F34")
            .PaddingVertical(9)
            .PaddingHorizontal(6)
            .DefaultTextStyle(style => style.FontColor(Colors.White).FontSize(8).Bold());
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .PaddingVertical(9)
            .PaddingHorizontal(6)
            .BorderBottom(1).BorderColor("#E7EEEB");
    }

    private static byte[]? LoadCompanyLogo()
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(LogoResourceName);
        if (stream is null)
        {
            return null;
        }

        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
