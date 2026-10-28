using System.Globalization;
using System.Net;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Services;

internal static class InvoiceEmailTemplate
{
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-US");
    private static readonly CultureInfo DateCulture = CultureInfo.GetCultureInfo("en-GB");

    public static string Render(
        Order order,
        string customerName,
        string customerEmail,
        string? frontendBaseUrl,
        string companyName,
        string companyPhone,
        string companyEmail,
        string companyWebsite,
        string companyAddress)
    {
        var safeCompanyName = Encode(companyName);
        var safeCustomerName = Encode(customerName);
        var safeCustomerEmail = Encode(customerEmail);
        var safeOrderNumber = Encode(order.OrderNumber);
        var safeCurrency = Encode(order.Currency);
        var invoiceDate = order.CreatedAt.ToString("dd MMM yyyy", DateCulture);
        var formattedTotal = order.TotalAmount.ToString("N2", DisplayCulture);
        var itemRows = string.Join(
            Environment.NewLine,
            order.Items.Select(item =>
            {
                var productName = Encode(item.ProductName);
                var strength = Encode(item.Strength ?? "-");
                var packSize = Encode(item.PackSize ?? "-");
                var quantity = item.Quantity.ToString("N0", DisplayCulture);
                var unitPrice = item.UnitPrice.ToString("N2", DisplayCulture);
                var lineTotal = (item.Quantity * item.UnitPrice).ToString("N2", DisplayCulture);

                return $"""
                    <tr>
                      <td style="padding:14px 12px;border-bottom:1px solid #e7eeeb;color:#172b4d;font-size:14px;line-height:21px;">
                        <strong style="display:block;">{productName}</strong>
                        <span style="display:block;margin-top:3px;color:#687784;font-size:12px;">{strength} · {packSize}</span>
                      </td>
                      <td align="center" style="padding:14px 8px;border-bottom:1px solid #e7eeeb;color:#334155;font-size:13px;">{quantity}</td>
                      <td align="right" style="padding:14px 8px;border-bottom:1px solid #e7eeeb;color:#334155;font-size:13px;white-space:nowrap;">{safeCurrency} {unitPrice}</td>
                      <td align="right" style="padding:14px 12px;border-bottom:1px solid #e7eeeb;color:#172b4d;font-size:13px;font-weight:700;white-space:nowrap;">{safeCurrency} {lineTotal}</td>
                    </tr>
                    """;
            }));

        var addressLines = new[]
        {
            order.ShippingAddressLine1,
            order.ShippingAddressLine2,
            string.Join(", ", new[] { order.ShippingCity, order.ShippingState }
                .Where(value => !string.IsNullOrWhiteSpace(value))),
            string.Join(" ", new[] { order.ShippingPostalCode, order.ShippingCountry }
                .Where(value => !string.IsNullOrWhiteSpace(value)))
        }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(Encode);
        var safeShippingAddress = string.Join("<br />", addressLines);
        if (string.IsNullOrWhiteSpace(safeShippingAddress))
        {
            safeShippingAddress = "Address not provided";
        }

        var logo = BuildLogoMarkup(frontendBaseUrl, safeCompanyName);
        var safePhone = Encode(companyPhone);
        var phoneHref = "tel:" + new string(companyPhone.Where(character => char.IsDigit(character) || character == '+').ToArray());
        var safeCompanyEmail = Encode(companyEmail);
        var safeWebsite = Encode(companyWebsite);
        var safeCompanyAddress = Encode(companyAddress);
        var contactLink = Encode($"mailto:{companyEmail}?subject={WebUtility.UrlEncode($"Order {order.OrderNumber}")}");

        return $"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width,initial-scale=1" />
              <meta name="color-scheme" content="light" />
              <title>Invoice {safeOrderNumber} | {safeCompanyName}</title>
            </head>
            <body style="margin:0;padding:0;background:#f2f6f4;font-family:Arial,Helvetica,sans-serif;color:#172b4d;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">Your Rashe Pharma invoice {safeOrderNumber} is attached.</div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;background:#f2f6f4;">
                <tr>
                  <td align="center" style="padding:28px 12px;">
                    <table role="presentation" width="640" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:640px;background:#ffffff;border:1px solid #e2eae6;border-radius:16px;overflow:hidden;">
                      <tr>
                        <td style="padding:24px 28px;background:#073f34;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                            <tr>
                              <td align="left" style="vertical-align:middle;">{logo}</td>
                              <td align="right" style="vertical-align:middle;"><span style="display:inline-block;padding:8px 12px;border:1px solid #ffffff40;border-radius:20px;color:#e9fff5;font-size:11px;font-weight:700;letter-spacing:1.4px;">PAYMENT RECEIPT</span></td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px 28px 18px;">
                          <p style="margin:0 0 8px;color:#0b8b68;font-size:11px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;">Thank you for your order</p>
                          <h1 style="margin:0;color:#172b4d;font-size:25px;line-height:32px;">Your invoice is ready</h1>
                          <p style="margin:12px 0 0;color:#64748b;font-size:14px;line-height:22px;">Dear {safeCustomerName}, your payment has been received. The detailed invoice PDF is attached to this email.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 22px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f6faf8;border:1px solid #e5eeea;border-radius:10px;">
                            <tr>
                              <td style="padding:14px 16px;"><span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Invoice number</span><strong style="display:block;margin-top:5px;color:#172b4d;font-size:14px;">{safeOrderNumber}</strong></td>
                              <td style="padding:14px 16px;"><span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Invoice date</span><strong style="display:block;margin-top:5px;color:#172b4d;font-size:14px;">{invoiceDate}</strong></td>
                              <td style="padding:14px 16px;"><span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Payment status</span><strong style="display:block;margin-top:5px;color:#08775a;font-size:14px;">Paid</strong></td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 8px;"><h2 style="margin:0;color:#172b4d;font-size:16px;">Order summary</h2></td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 18px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;border:1px solid #e5eeea;border-radius:10px;border-collapse:separate;border-spacing:0;overflow:hidden;">
                            <thead><tr style="background:#f6faf8;"><th align="left" style="padding:12px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Product</th><th align="center" style="padding:12px 8px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Qty</th><th align="right" style="padding:12px 8px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Unit price</th><th align="right" style="padding:12px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Amount</th></tr></thead>
                            <tbody>{itemRows}</tbody>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 24px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0"><tr><td style="color:#64748b;font-size:13px;">Order: <strong style="color:#334155;">{safeOrderNumber}</strong></td><td align="right" style="color:#172b4d;font-size:14px;">Total paid<br /><strong style="font-size:22px;line-height:32px;">{safeCurrency} {formattedTotal}</strong></td></tr></table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 24px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f6faf8;border:1px solid #e5eeea;border-radius:10px;"><tr><td style="padding:16px 18px;"><h2 style="margin:0 0 8px;color:#172b4d;font-size:14px;">Shipping address</h2><p style="margin:0;color:#52616b;font-size:13px;line-height:20px;">{safeCustomerName}<br />{safeCustomerEmail}<br />{safeShippingAddress}</p></td></tr></table>
                        </td>
                      </tr>
                      <tr>
                        <td align="center" style="padding:0 28px 28px;">
                          <a href="{contactLink}" style="display:inline-block;padding:13px 22px;border-radius:7px;background:#07865f;color:#ffffff;text-decoration:none;font-size:14px;font-weight:700;">Contact our team</a>
                          <p style="margin:14px 0 0;color:#718096;font-size:12px;line-height:19px;">Your invoice PDF is attached for your records.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:22px 28px;background:#f6faf8;border-top:1px solid #e5eeea;">
                          <p style="margin:0 0 12px;color:#172b4d;font-size:13px;font-weight:700;">{safeCompanyName}</p>
                          <p style="margin:0 0 7px;color:#52616b;font-size:12px;line-height:18px;"><a href="{Encode(phoneHref)}" style="color:#08775a;text-decoration:none;">{safePhone}</a>&nbsp; · &nbsp;<a href="mailto:{safeCompanyEmail}" style="color:#08775a;text-decoration:none;">{safeCompanyEmail}</a></p>
                          <p style="margin:0 0 7px;color:#52616b;font-size:12px;line-height:18px;">{safeCompanyAddress}</p>
                          <a href="{safeWebsite}" style="color:#08775a;font-size:12px;text-decoration:none;">{safeWebsite}</a>
                          <p style="margin:16px 0 0;color:#84918d;font-size:10px;line-height:16px;">This is an order receipt. Refer to the attached PDF for the complete invoice.</p>
                        </td>
                      </tr>
                    </table>
                    <p style="margin:14px 0 0;color:#87928e;font-size:10px;line-height:16px;">{safeCompanyName} · Quality healthcare, built for trust</p>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;
    }

    private static string BuildLogoMarkup(string? frontendBaseUrl, string safeCompanyName)
    {
        if (Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var baseUri) &&
            string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            var logoUri = new Uri(baseUri, "/images/Rashelifescience.png");
            return $"""
                <img src="{Encode(logoUri.AbsoluteUri)}" width="170" alt="{safeCompanyName}" style="display:block;width:170px;max-width:100%;height:auto;border:0;" />
                """;
        }

        return $"""<span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:.2px;">{safeCompanyName}</span>""";
    }

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
