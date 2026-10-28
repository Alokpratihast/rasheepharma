using System.Globalization;
using System.Net;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Services;

internal static class QuotationEmailTemplate
{
    private static readonly CultureInfo DisplayCulture =
        CultureInfo.GetCultureInfo("en-US");

    public static string Render(
        Quotation quotation,
        string customerName,
        string? frontendBaseUrl,
        string companyName,
        string companyPhone,
        string companyEmail,
        string companyWebsite,
        string companyAddress)
    {
        var safeCompanyName = Encode(companyName);
        var safeCustomerName = Encode(customerName);
        var safeQuoteNumber = Encode(quotation.QuoteNumber);
        var safeCurrency = Encode(quotation.Currency);
        var issueDate = quotation.CreatedAt.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("en-GB"));
        var validUntil = quotation.ValidUntil.ToString("dd MMM yyyy", CultureInfo.GetCultureInfo("en-GB"));
        var formattedTotal = quotation.TotalAmount.ToString("N2", DisplayCulture);

        var itemRows = string.Join(
            Environment.NewLine,
            quotation.Items.Select(item =>
            {
                var productName = Encode(item.ProductVariant.Product.Name);
                var strength = Encode(item.ProductVariant.Strength ?? "-");
                var packSize = Encode(item.ProductVariant.PackSize ?? "-");
                var quantity = item.Quantity.ToString("N0", DisplayCulture);
                var unitPrice = item.UnitPrice.ToString("N2", DisplayCulture);
                var totalPrice = item.TotalPrice.ToString("N2", DisplayCulture);

                return $"""
                    <tr>
                      <td style="padding:16px 12px;border-bottom:1px solid #e7eeeb;color:#172b4d;font-size:14px;line-height:21px;">
                        <strong style="display:block;color:#172b4d;">{productName}</strong>
                        <span style="display:block;margin-top:3px;color:#687784;font-size:12px;">{strength} · {packSize}</span>
                      </td>
                      <td align="center" style="padding:16px 8px;border-bottom:1px solid #e7eeeb;color:#334155;font-size:13px;">{quantity}</td>
                      <td align="right" style="padding:16px 8px;border-bottom:1px solid #e7eeeb;color:#334155;font-size:13px;white-space:nowrap;">{safeCurrency} {unitPrice}</td>
                      <td align="right" style="padding:16px 12px;border-bottom:1px solid #e7eeeb;color:#172b4d;font-size:13px;font-weight:700;white-space:nowrap;">{safeCurrency} {totalPrice}</td>
                    </tr>
                    """;
            }));

        if (string.IsNullOrWhiteSpace(itemRows))
        {
            itemRows = """
                <tr>
                  <td colspan="4" style="padding:18px 12px;color:#687784;font-size:14px;">No products were included in this quotation.</td>
                </tr>
                """;
        }

        var notes = string.IsNullOrWhiteSpace(quotation.Notes)
            ? string.Empty
            : $"""
                <tr>
                  <td style="padding:0 28px 24px;">
                    <div style="padding:16px 18px;border-left:3px solid #0b8b68;background:#f2f8f5;color:#475569;font-size:13px;line-height:21px;">
                      <strong style="color:#172b4d;">Additional notes</strong><br />
                      {Encode(quotation.Notes).Replace("\r\n", "<br />").Replace("\n", "<br />")}
                    </div>
                  </td>
                </tr>
                """;

        var logo = BuildLogoMarkup(frontendBaseUrl, safeCompanyName);
        var safePhone = Encode(companyPhone);
        var phoneHref = "tel:" + new string(companyPhone.Where(character => char.IsDigit(character) || character == '+').ToArray());
        var safeEmail = Encode(companyEmail);
        var safeWebsite = Encode(companyWebsite);
        var safeAddress = Encode(companyAddress);
        var replySubject = WebUtility.UrlEncode($"Quotation {quotation.QuoteNumber}");
        var replyLink = Encode($"mailto:{companyEmail}?subject={replySubject}");

        return $"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width,initial-scale=1" />
              <meta name="color-scheme" content="light" />
              <title>Quotation {safeQuoteNumber} | {safeCompanyName}</title>
            </head>
            <body style="margin:0;padding:0;background:#f2f6f4;font-family:Arial,Helvetica,sans-serif;color:#172b4d;">
              <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">Your Rashe Pharma quotation {safeQuoteNumber} is ready for review.</div>
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;background:#f2f6f4;">
                <tr>
                  <td align="center" style="padding:28px 12px;">
                    <table role="presentation" width="640" cellspacing="0" cellpadding="0" border="0" style="width:100%;max-width:640px;background:#ffffff;border:1px solid #e2eae6;border-radius:16px;overflow:hidden;">
                      <tr>
                        <td style="padding:24px 28px;background:#073f34;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                            <tr>
                              <td align="left" style="vertical-align:middle;">{logo}</td>
                              <td align="right" style="vertical-align:middle;">
                                <span style="display:inline-block;padding:8px 12px;border:1px solid #ffffff40;border-radius:20px;color:#e9fff5;font-size:11px;font-weight:700;letter-spacing:1.4px;">PRICE QUOTATION</span>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px 28px 18px;">
                          <p style="margin:0 0 8px;color:#0b8b68;font-size:11px;font-weight:700;letter-spacing:1.5px;text-transform:uppercase;">Prepared for {safeCustomerName}</p>
                          <h1 style="margin:0;color:#172b4d;font-size:25px;line-height:32px;">Your quotation is ready</h1>
                          <p style="margin:12px 0 0;color:#64748b;font-size:14px;line-height:22px;">Thank you for your enquiry. Please review the product and pricing details below. Our company brochure is attached for your reference.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 22px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#f6faf8;border:1px solid #e5eeea;border-radius:10px;">
                            <tr>
                              <td style="padding:14px 16px;">
                                <span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Quotation number</span>
                                <strong style="display:block;margin-top:5px;color:#172b4d;font-size:14px;">{safeQuoteNumber}</strong>
                              </td>
                              <td style="padding:14px 16px;">
                                <span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Issued</span>
                                <strong style="display:block;margin-top:5px;color:#172b4d;font-size:14px;">{issueDate}</strong>
                              </td>
                              <td style="padding:14px 16px;">
                                <span style="display:block;color:#718096;font-size:11px;text-transform:uppercase;letter-spacing:.6px;">Valid until</span>
                                <strong style="display:block;margin-top:5px;color:#172b4d;font-size:14px;">{validUntil}</strong>
                              </td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 8px;">
                          <h2 style="margin:0;color:#172b4d;font-size:16px;">Quotation details</h2>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 20px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="width:100%;border:1px solid #e5eeea;border-radius:10px;border-collapse:separate;border-spacing:0;overflow:hidden;">
                            <thead>
                              <tr style="background:#f6faf8;">
                                <th align="left" style="padding:12px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Product</th>
                                <th align="center" style="padding:12px 8px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Qty</th>
                                <th align="right" style="padding:12px 8px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Unit price</th>
                                <th align="right" style="padding:12px;color:#64748b;font-size:10px;letter-spacing:.6px;text-transform:uppercase;">Amount</th>
                              </tr>
                            </thead>
                            <tbody>{itemRows}</tbody>
                          </table>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 28px 24px;">
                          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0">
                            <tr>
                              <td style="color:#64748b;font-size:13px;">Currency: <strong style="color:#334155;">{safeCurrency}</strong></td>
                              <td align="right" style="color:#172b4d;font-size:14px;">Total amount<br /><strong style="font-size:22px;line-height:32px;">{safeCurrency} {formattedTotal}</strong></td>
                            </tr>
                          </table>
                        </td>
                      </tr>
                      {notes}
                      <tr>
                        <td align="center" style="padding:0 28px 28px;">
                          <a href="{replyLink}" style="display:inline-block;padding:13px 22px;border-radius:7px;background:#07865f;color:#ffffff;text-decoration:none;font-size:14px;font-weight:700;">Reply to discuss this quotation</a>
                          <p style="margin:14px 0 0;color:#718096;font-size:12px;line-height:19px;">Please contact our team if you have any questions about this quotation.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:22px 28px;background:#f6faf8;border-top:1px solid #e5eeea;">
                          <p style="margin:0 0 12px;color:#172b4d;font-size:13px;font-weight:700;">{safeCompanyName}</p>
                          <p style="margin:0 0 7px;color:#52616b;font-size:12px;line-height:18px;">
                            <a href="{Encode(phoneHref)}" style="color:#08775a;text-decoration:none;">{safePhone}</a>
                            &nbsp; · &nbsp;
                            <a href="mailto:{safeEmail}" style="color:#08775a;text-decoration:none;">{safeEmail}</a>
                          </p>
                          <p style="margin:0 0 7px;color:#52616b;font-size:12px;line-height:18px;">{safeAddress}</p>
                          <a href="{safeWebsite}" style="color:#08775a;font-size:12px;text-decoration:none;">{safeWebsite}</a>
                          <p style="margin:16px 0 0;color:#84918d;font-size:10px;line-height:16px;">This quotation is valid through the date shown above. Please contact us for clarification or changes.</p>
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
        if (Uri.TryCreate(frontendBaseUrl, UriKind.Absolute, out var baseUri)
            && string.Equals(baseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            var logoUri = new Uri(baseUri, "/images/Rashelifescience.png");
            return $"""
                <img src="{Encode(logoUri.AbsoluteUri)}" width="170" alt="{safeCompanyName}" style="display:block;width:170px;max-width:100%;height:auto;border:0;" />
                """;
        }

        return $"""<span style="color:#ffffff;font-size:20px;font-weight:700;letter-spacing:.2px;">{safeCompanyName}</span>""";
    }

    private static string Encode(string? value) =>
        WebUtility.HtmlEncode(value ?? string.Empty);
}
