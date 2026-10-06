using ClosedXML.Excel;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class ClosedXmlBulkUploadExcelParser
    : IBulkUploadExcelParser
{
    public async Task<BulkUploadExcelData> ParseAsync(
        Stream excelStream,
        CancellationToken cancellationToken = default)
    {
        if (excelStream == null)
        {
            throw new ArgumentNullException(nameof(excelStream));
        }

        if (!excelStream.CanRead)
        {
            throw new InvalidOperationException(
                "The Excel stream cannot be read.");
        }

        using var workbook = new XLWorkbook(excelStream);

        var result = new BulkUploadExcelData();

        ParseProducts(
            workbook,
            result,
            cancellationToken);

        ParseVariants(
            workbook,
            result,
            cancellationToken);

        ParseImages(
            workbook,
            result,
            cancellationToken);

        await Task.CompletedTask;

        return result;
    }

    private static void ParseProducts(
        XLWorkbook workbook,
        BulkUploadExcelData result,
        CancellationToken cancellationToken)
    {
        var worksheet = GetRequiredWorksheet(
            workbook,
            "Products");

        var headers = ReadHeaders(worksheet);

        ValidateHeaders(
            headers,
            "Products",
            new[]
            {
                "Category",
                "Product Name",
                "Slug"
            });

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsEmptyRow(worksheet, rowNumber))
            {
                continue;
            }

            var row = new BulkUploadProductRow
            {
                Category = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Category"),

                ProductName = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Product Name"),

                Slug = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Slug"),

                GenericName = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Generic Name"),

                Composition = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Composition"),

                DosageForm = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Dosage Form"),

                Description = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Description"),

                BrandName = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Brand Name"),

                Manufacturer = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Manufacturer"),

                IsActive = GetBool(
                    worksheet,
                    rowNumber,
                    headers,
                    "Is Active"),

                IsFeatured = GetBool(
                    worksheet,
                    rowNumber,
                    headers,
                    "Is Featured")
            };

            result.Products.Add(row);
        }
    }

    private static void ParseVariants(
        XLWorkbook workbook,
        BulkUploadExcelData result,
        CancellationToken cancellationToken)
    {
        var worksheet = GetRequiredWorksheet(
            workbook,
            "Variants");

        var headers = ReadHeaders(worksheet);

        ValidateHeaders(
            headers,
            "Variants",
            new[]
            {
                "Product Slug",
                "Price INR"
            });

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsEmptyRow(worksheet, rowNumber))
            {
                continue;
            }

            var row = new BulkUploadVariantRow
            {
                ProductSlug = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Product Slug"),

                Strength = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Strength"),

                PackSize = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Pack Size"),

                PriceInr = GetDecimal(
                    worksheet,
                    rowNumber,
                    headers,
                    "Price INR"),

                MOQ = GetInt(
                    worksheet,
                    rowNumber,
                    headers,
                    "MOQ"),

                UnitType = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Unit Type"),

                SKU = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "SKU"),

                StockQuantity = GetInt(
                    worksheet,
                    rowNumber,
                    headers,
                    "Stock Quantity"),

                IsActive = GetBool(
                    worksheet,
                    rowNumber,
                    headers,
                    "Is Active")
            };

            result.Variants.Add(row);
        }
    }

    private static void ParseImages(
        XLWorkbook workbook,
        BulkUploadExcelData result,
        CancellationToken cancellationToken)
    {
        var worksheet = GetRequiredWorksheet(
            workbook,
            "Images");

        var headers = ReadHeaders(worksheet);

        ValidateHeaders(
            headers,
            "Images",
            new[]
            {
                "Product Slug",
                "Image File Name"
            });

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (IsEmptyRow(worksheet, rowNumber))
            {
                continue;
            }

            var row = new BulkUploadImageRow
            {
                ProductSlug = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Product Slug"),

                ImageFileName = GetString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Image File Name"),

                AltText = GetNullableString(
                    worksheet,
                    rowNumber,
                    headers,
                    "Alt Text"),

                IsPrimary = GetBool(
                    worksheet,
                    rowNumber,
                    headers,
                    "Is Primary"),

                DisplayOrder = GetInt(
                    worksheet,
                    rowNumber,
                    headers,
                    "Display Order")
            };

            result.Images.Add(row);
        }
    }

    private static IXLWorksheet GetRequiredWorksheet(
        XLWorkbook workbook,
        string worksheetName)
    {
        var worksheet = workbook.Worksheets
            .FirstOrDefault(
                x => string.Equals(
                    x.Name.Trim(),
                    worksheetName,
                    StringComparison.OrdinalIgnoreCase));

        if (worksheet == null)
        {
            throw new InvalidOperationException(
                $"Required Excel sheet '{worksheetName}' was not found.");
        }

        return worksheet;
    }

    private static Dictionary<string, int> ReadHeaders(
        IXLWorksheet worksheet)
    {
        var headerRow = worksheet.FirstRowUsed();

        if (headerRow == null)
        {
            throw new InvalidOperationException(
                $"Excel sheet '{worksheet.Name}' is empty.");
        }

        var headers = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var header = cell.GetString().Trim();

            if (string.IsNullOrWhiteSpace(header))
            {
                continue;
            }

            if (headers.ContainsKey(header))
            {
                throw new InvalidOperationException(
                    $"Duplicate column '{header}' found in sheet '{worksheet.Name}'.");
            }

            headers[header] = cell.Address.ColumnNumber;
        }

        return headers;
    }

    private static void ValidateHeaders(
        Dictionary<string, int> headers,
        string worksheetName,
        IEnumerable<string> requiredHeaders)
    {
        foreach (var requiredHeader in requiredHeaders)
        {
            if (!headers.ContainsKey(requiredHeader))
            {
                throw new InvalidOperationException(
                    $"Required column '{requiredHeader}' is missing " +
                    $"from sheet '{worksheetName}'.");
            }
        }
    }

    private static bool IsEmptyRow(
        IXLWorksheet worksheet,
        int rowNumber)
    {
        return !worksheet.Row(rowNumber)
            .CellsUsed()
            .Any();
    }

    private static string GetString(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        var value = GetCellValue(
            worksheet,
            rowNumber,
            headers,
            columnName);

        return value.Trim();
    }

    private static string? GetNullableString(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        if (!headers.ContainsKey(columnName))
        {
            return null;
        }

        var value = GetCellValue(
            worksheet,
            rowNumber,
            headers,
            columnName);

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static decimal GetDecimal(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        var cell = GetCell(
            worksheet,
            rowNumber,
            headers,
            columnName);

        if (cell.IsEmpty())
        {
            return 0;
        }

        if (cell.TryGetValue<decimal>(out var value))
        {
            return value;
        }

        var text = cell.GetString().Trim();

        if (decimal.TryParse(
                text,
                out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException(
            $"Invalid decimal value '{text}' " +
            $"for column '{columnName}' in sheet " +
            $"'{worksheet.Name}', row {rowNumber}.");
    }

    private static int GetInt(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        var cell = GetCell(
            worksheet,
            rowNumber,
            headers,
            columnName);

        if (cell.IsEmpty())
        {
            return 0;
        }

        if (cell.TryGetValue<int>(out var value))
        {
            return value;
        }

        var text = cell.GetString().Trim();

        if (int.TryParse(
                text,
                out var parsedValue))
        {
            return parsedValue;
        }

        throw new InvalidOperationException(
            $"Invalid integer value '{text}' " +
            $"for column '{columnName}' in sheet " +
            $"'{worksheet.Name}', row {rowNumber}.");
    }

    private static bool GetBool(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        if (!headers.ContainsKey(columnName))
        {
            return false;
        }

        var cell = GetCell(
            worksheet,
            rowNumber,
            headers,
            columnName);

        if (cell.IsEmpty())
        {
            return false;
        }

        if (cell.TryGetValue<bool>(out var value))
        {
            return value;
        }

        var text = cell.GetString()
            .Trim()
            .ToLowerInvariant();

        return text switch
        {
            "true" => true,
            "yes" => true,
            "1" => true,
            "y" => true,

            "false" => false,
            "no" => false,
            "0" => false,
            "n" => false,

            _ => throw new InvalidOperationException(
                $"Invalid boolean value '{text}' " +
                $"for column '{columnName}' in sheet " +
                $"'{worksheet.Name}', row {rowNumber}.")
        };
    }

    private static string GetCellValue(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        var cell = GetCell(
            worksheet,
            rowNumber,
            headers,
            columnName);

        return cell.GetString();
    }

    private static IXLCell GetCell(
        IXLWorksheet worksheet,
        int rowNumber,
        Dictionary<string, int> headers,
        string columnName)
    {
        if (!headers.TryGetValue(
                columnName,
                out var columnNumber))
        {
            throw new InvalidOperationException(
                $"Column '{columnName}' was not found " +
                $"in sheet '{worksheet.Name}'.");
        }

        return worksheet.Cell(
            rowNumber,
            columnNumber);
    }
}