using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly CategoryRepository _repository;

    public CategoryRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new CategoryRepository(_context);
    }

    // =========================================================
    // GetAllAsync
    // =========================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        var category1 = CreateCategory("Antibiotics", "antibiotics");
        var category2 = CreateCategory("Gynae", "gynae");

        _context.Categories.AddRange(category1, category2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Contains(result, c => c.Name == "Antibiotics");
        Assert.Contains(result, c => c.Name == "Gynae");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategories()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenFound()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Antibiotics", result.Name);
        Assert.Equal("antibiotics", result.Slug);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(99999);

        Assert.Null(result);
    }

    // =========================================================
    // GetBySlugAsync
    // =========================================================

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnCategory_WhenFound()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _repository.GetBySlugAsync("antibiotics");

        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Antibiotics", result.Name);
        Assert.Equal("antibiotics", result.Slug);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetBySlugAsync(
            "does-not-exist");

        Assert.Null(result);
    }

    // =========================================================
    // AddAsync
    // =========================================================

    [Fact]
    public async Task AddAsync_ShouldAddCategory()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        await _repository.AddAsync(category);
        await _context.SaveChangesAsync();

        var savedCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == "antibiotics");

        Assert.NotNull(savedCategory);
        Assert.Equal("Antibiotics", savedCategory.Name);
        Assert.Equal("antibiotics", savedCategory.Slug);
    }

    // =========================================================
    // UpdateAsync
    // =========================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        category.Name = "Updated Antibiotics";
        category.Slug = "updated-antibiotics";

        await _repository.UpdateAsync(category);
        await _context.SaveChangesAsync();

        var updatedCategory = await _context.Categories
            .AsNoTracking()
            .FirstAsync(c => c.Id == category.Id);

        Assert.Equal(
            "Updated Antibiotics",
            updatedCategory.Name);

        Assert.Equal(
            "updated-antibiotics",
            updatedCategory.Slug);
    }

    // =========================================================
    // DeleteAsync
    // =========================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeleteCategory()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(category);
        await _context.SaveChangesAsync();

        var deletedCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);

        Assert.Null(deletedCategory);
    }

    // =========================================================
    // ExistsBySlugAsync
    // =========================================================

    [Fact]
    public async Task ExistsBySlugAsync_ShouldReturnTrue_WhenSlugExists()
    {
        var category = CreateCategory(
            "Antibiotics",
            "antibiotics");

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsBySlugAsync(
            "antibiotics");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsBySlugAsync_ShouldReturnFalse_WhenSlugDoesNotExist()
    {
        var result = await _repository.ExistsBySlugAsync(
            "does-not-exist");

        Assert.False(result);
    }

    // =========================================================
    // Helper
    // =========================================================

    private static Category CreateCategory(
        string name,
        string slug)
    {
        return new Category
        {
            Name = name,
            Slug = slug,
            Description = $"Test description for {name}",
            IsActive = true
        };
    }

    // =========================================================
    // Dispose
    // =========================================================

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}