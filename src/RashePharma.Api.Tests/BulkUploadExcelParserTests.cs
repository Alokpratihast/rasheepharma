
using ClosedXML.Excel;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadExcelParserTests
{
    private readonly ClosedXmlBulkUploadExcelParser _parser = new();

    [Fact]
    public async Task ParseAsync_ValidWorkbook_ShouldParseAllRecords()
    {
        using var workbook = CreateValidWorkbook();
        using var stream = new MemoryStream();

        workbook.SaveAs(stream);
        stream.Position = 0;

        var result = await _parser.ParseAsync(stream);

        Assert.Single(result.Products);
        Assert.Single(result.Variants);
        Assert.Single(result.Images);

        Assert.Equal("Test Product", result.Products[0].ProductName);
        Assert.Equal("test-product", result.Products[0].Slug);

        Assert.Equal("test-product", result.Variants[0].ProductSlug);
        Assert.Equal(100m, result.Variants[0].PriceInr);

        Assert.Equal("test-product", result.Images[0].ProductSlug);
        Assert.Equal("test.jpg", result.Images[0].ImageFileName);
    }

    [Fact]
    public async Task ParseAsync_MissingProductsSheet_ShouldThrowException()
    {
        using var workbook = new XLWorkbook();

        AddVariantsSheet(workbook);
        AddImagesSheet(workbook);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _parser.ParseAsync(stream));

        Assert.Contains("Products", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_MissingProductNameColumn_ShouldThrowException()
    {
        using var workbook = new XLWorkbook();

        var products = workbook.Worksheets.Add("Products");
        products.Cell(1, 1).Value = "Category";
        products.Cell(1, 2).Value = "Slug";

        AddVariantsSheet(workbook);
        AddImagesSheet(workbook);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _parser.ParseAsync(stream));

        Assert.Contains("Product Name", exception.Message);
    }

    private static XLWorkbook CreateValidWorkbook()
    {
        var workbook = new XLWorkbook();

        var products = workbook.Worksheets.Add("Products");
        products.Cell(1, 1).Value = "Category";
        products.Cell(1, 2).Value = "Product Name";
        products.Cell(1, 3).Value = "Slug";

        products.Cell(2, 1).Value = "Medicine";
        products.Cell(2, 2).Value = "Test Product";
        products.Cell(2, 3).Value = "test-product";

        AddVariantsSheet(workbook, addData: true);
        AddImagesSheet(workbook, addData: true);

        return workbook;
    }

   private static void AddVariantsSheet(
    XLWorkbook workbook,
    bool addData = false)
{
    var sheet = workbook.Worksheets.Add("Variants");

    sheet.Cell(1, 1).Value = "Product Slug";
    sheet.Cell(1, 2).Value = "Strength";
    sheet.Cell(1, 3).Value = "Pack Size";
    sheet.Cell(1, 4).Value = "Price INR";
    sheet.Cell(1, 5).Value = "MOQ";
    sheet.Cell(1, 6).Value = "Unit Type";
    sheet.Cell(1, 7).Value = "SKU";
    sheet.Cell(1, 8).Value = "Stock Quantity";
    sheet.Cell(1, 9).Value = "Is Active";

    if (addData)
    {
        sheet.Cell(2, 1).Value = "test-product";
        sheet.Cell(2, 2).Value = "500mg";
        sheet.Cell(2, 3).Value = "10 Tablets";
        sheet.Cell(2, 4).Value = 100;
        sheet.Cell(2, 5).Value = 1;
        sheet.Cell(2, 6).Value = "Strip";
        sheet.Cell(2, 7).Value = "TEST-001";
        sheet.Cell(2, 8).Value = 100;
        sheet.Cell(2, 9).Value = true;
    }
}

   private static void AddImagesSheet(
    XLWorkbook workbook,
    bool addData = false)
{
    var sheet = workbook.Worksheets.Add("Images");

    sheet.Cell(1, 1).Value = "Product Slug";
    sheet.Cell(1, 2).Value = "Image File Name";
    sheet.Cell(1, 3).Value = "Alt Text";
    sheet.Cell(1, 4).Value = "Is Primary";
    sheet.Cell(1, 5).Value = "Display Order";

    if (addData)
    {
        sheet.Cell(2, 1).Value = "test-product";
        sheet.Cell(2, 2).Value = "test.jpg";
        sheet.Cell(2, 3).Value = "Test product image";
        sheet.Cell(2, 4).Value = true;
        sheet.Cell(2, 5).Value = 1;
    }
}
}
