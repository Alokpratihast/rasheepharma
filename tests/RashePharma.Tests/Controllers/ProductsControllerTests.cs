using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Products;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;
using Xunit;

namespace RashePharma.Tests.Controllers;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ---------------------------------------------------------
    // Helper: Create a Category for Product tests
    // ---------------------------------------------------------

    private async Task<int> CreateCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var category = new Category
        {
            Name = $"Test Category {Guid.NewGuid():N}",
            Slug = $"test-category-{Guid.NewGuid():N}",
            Description = "Category created for product controller tests",
            IsActive = true
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return category.Id;
    }

    // ---------------------------------------------------------
    // Helper: Create a Product directly in test database
    // ---------------------------------------------------------

    private async Task<int> CreateProductAsync(int categoryId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var product = new Product
        {
            Name = $"Test Product {Guid.NewGuid():N}",
            Slug = $"test-product-{Guid.NewGuid():N}",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test product description",
            Manufacturer = "Test Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return product.Id;
    }

    // =========================================================
    // GET /api/products
    // =========================================================

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // =========================================================
    // GET /api/products/{id}
    // =========================================================

    [Fact]
    public async Task GetById_WhenProductExists_ShouldReturnOk()
    {
        var categoryId = await CreateCategoryAsync();
        var productId = await CreateProductAsync(categoryId);

        var response = await _client.GetAsync(
            $"/api/products/{productId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync(
            "/api/products/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =========================================================
    // GET /api/products/slug/{slug}
    // =========================================================

    [Fact]
    public async Task GetBySlug_WhenProductExists_ShouldReturnOk()
    {
        var categoryId = await CreateCategoryAsync();

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var slug = $"test-product-{Guid.NewGuid():N}";

        var product = new Product
        {
            Name = "Test Product",
            Slug = slug,
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test Description",
            Manufacturer = "Test Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        var response = await _client.GetAsync(
            $"/api/products/slug/{slug}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBySlug_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync(
            "/api/products/slug/product-that-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =========================================================
    // POST /api/products
    // =========================================================

    [Fact]
    public async Task Create_WhenValidRequest_ShouldReturnCreated()
    {
        var categoryId = await CreateCategoryAsync();

        var request = new ProductCreateDto
        {
            Name = "New Test Product",
            Slug = $"new-test-product-{Guid.NewGuid():N}",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test Description",
            Manufacturer = "Test Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenCategoryDoesNotExist_ShouldReturnBadRequest()
    {
        var request = new ProductCreateDto
        {
            Name = "Invalid Product",
            Slug = $"invalid-product-{Guid.NewGuid():N}",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test Description",
            Manufacturer = "Test Manufacturer",
            CategoryId = 99999,
            IsActive = true
        };

        var response = await _client.PostAsJsonAsync(
            "/api/products",
            request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenSlugAlreadyExists_ShouldReturnBadRequest()
    {
        var categoryId = await CreateCategoryAsync();

        var slug = $"duplicate-product-{Guid.NewGuid():N}";

        var firstRequest = new ProductCreateDto
        {
            Name = "First Product",
            Slug = slug,
            GenericName = "Generic",
            Composition = "Composition",
            DosageForm = "Tablet",
            Description = "Description",
            Manufacturer = "Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/products",
            firstRequest);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var secondRequest = new ProductCreateDto
        {
            Name = "Second Product",
            Slug = slug,
            GenericName = "Generic",
            Composition = "Composition",
            DosageForm = "Tablet",
            Description = "Description",
            Manufacturer = "Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        var secondResponse = await _client.PostAsJsonAsync(
            "/api/products",
            secondRequest);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }

    // =========================================================
    // PUT /api/products/{id}
    // =========================================================

    [Fact]
    public async Task Update_WhenProductExists_ShouldReturnOk()
    {
        var categoryId = await CreateCategoryAsync();
        var productId = await CreateProductAsync(categoryId);

        var request = new ProductUpdateDto
        {
            Name = "Updated Product",
            Slug = $"updated-product-{Guid.NewGuid():N}",
            GenericName = "Updated Generic",
            Composition = "Updated Composition",
            DosageForm = "Capsule",
            Description = "Updated Description",
            Manufacturer = "Updated Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync(
            $"/api/products/{productId}",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var categoryId = await CreateCategoryAsync();

        var request = new ProductUpdateDto
        {
            Name = "Updated Product",
            Slug = $"updated-product-{Guid.NewGuid():N}",
            GenericName = "Updated Generic",
            Composition = "Updated Composition",
            DosageForm = "Capsule",
            Description = "Updated Description",
            Manufacturer = "Updated Manufacturer",
            CategoryId = categoryId,
            IsActive = true
        };

        var response = await _client.PutAsJsonAsync(
            "/api/products/99999",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // DELETE /api/products/{id}
    // =========================================================

    [Fact]
    public async Task Delete_WhenProductExists_ShouldReturnNoContent()
    {
        var categoryId = await CreateCategoryAsync();
        var productId = await CreateProductAsync(categoryId);

        var response = await _client.DeleteAsync(
            $"/api/products/{productId}");

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    [Fact]
    public async Task Delete_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync(
            "/api/products/99999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}