using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Categories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;
using Xunit;

namespace RashePharma.Tests.Controllers;

public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<int> CreateCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = new Category
        {
            Name = $"Test Category {Guid.NewGuid():N}",
            Slug = $"test-category-{Guid.NewGuid():N}",
            Description = "Category created for controller tests",
            IsActive = true
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return category.Id;
    }

    // ---------------------------------------------------------
    // GET ALL
    // ---------------------------------------------------------

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        // Arrange
        await CreateCategoryAsync();

        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories =
            await response.Content.ReadFromJsonAsync<List<CategoryListDto>>();

        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
    }

    // ---------------------------------------------------------
    // GET BY ID
    // ---------------------------------------------------------

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync();

        // Act
        var response =
            await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category =
            await response.Content.ReadFromJsonAsync<CategoryDetailsDto>();

        Assert.NotNull(category);
        Assert.Equal(categoryId, category.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        const int categoryId = 999999;

        // Act
        var response =
            await _client.GetAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------
    // GET BY SLUG
    // ---------------------------------------------------------

    [Fact]
    public async Task GetBySlug_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var category = await db.Categories.FindAsync(categoryId);

        Assert.NotNull(category);

        // Act
        var response =
            await _client.GetAsync($"/api/categories/slug/{category!.Slug}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content.ReadFromJsonAsync<CategoryDetailsDto>();

        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
        Assert.Equal(category.Slug, result.Slug);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        const string slug = "category-that-does-not-exist";

        // Act
        var response =
            await _client.GetAsync($"/api/categories/slug/{slug}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------
    // CREATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        var dto = new CategoryCreateDto
        {
            Name = $"Created Category {Guid.NewGuid():N}",
            Slug = $"created-category-{Guid.NewGuid():N}",
            Description = "Created through controller test",
            IsActive = true
        };

        // Act
        var response =
            await _client.PostAsJsonAsync("/api/categories", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var category =
            await response.Content.ReadFromJsonAsync<CategoryDetailsDto>();

        Assert.NotNull(category);
        Assert.True(category.Id > 0);
        Assert.Equal(dto.Name, category.Name);
        Assert.Equal(dto.Slug, category.Slug);
    }

    // ---------------------------------------------------------
    // UPDATE
    // ---------------------------------------------------------

    [Fact]
    public async Task Update_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync();

        var dto = new CategoryUpdateDto
        {
            Name = $"Updated Category {Guid.NewGuid():N}",
            Slug = $"updated-category-{Guid.NewGuid():N}",
            Description = "Updated through controller test",
            IsActive = true
        };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var category =
            await response.Content.ReadFromJsonAsync<CategoryDetailsDto>();

        Assert.NotNull(category);
        Assert.Equal(categoryId, category.Id);
        Assert.Equal(dto.Name, category.Name);
        Assert.Equal(dto.Slug, category.Slug);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        const int categoryId = 999999;

        var dto = new CategoryUpdateDto
        {
            Name = "Updated Category",
            Slug = $"updated-category-{Guid.NewGuid():N}",
            Description = "Updated category",
            IsActive = true
        };

        // Act
        var response =
            await _client.PutAsJsonAsync(
                $"/api/categories/{categoryId}",
                dto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------------------------------------------------------
    // DELETE
    // ---------------------------------------------------------

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenCategoryExists()
    {
        // Arrange
        var categoryId = await CreateCategoryAsync();

        // Act
        var response =
            await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        const int categoryId = 999999;

        // Act
        var response =
            await _client.DeleteAsync($"/api/categories/{categoryId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}