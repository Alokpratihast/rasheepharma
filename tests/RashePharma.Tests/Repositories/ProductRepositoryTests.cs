using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product1 = CreateProduct("Product One", "product-one", category);
        var product2 = CreateProduct("Product Two", "product-two", category);

        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Product One");
        Assert.Contains(result, p => p.Name == "Product Two");
    }

    [Fact]
    public async Task GetAllAsync_ShouldLoadCategoryVariantsAndImages()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        product.Variants.Add(new ProductVariant
        {
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = 120,
            SKU = "SKU-001",
            StockQuantity = 50,
            IsActive = true
        });

        product.Images.Add(new ProductImage
        {
            ImageUrl = "/images/product-one.jpg",
            AltText = "Product One",
            IsPrimary = true,
            DisplayOrder = 1
        });

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        var loadedProduct = Assert.Single(result);

        Assert.NotNull(loadedProduct.Category);
        Assert.Equal("Antibiotics", loadedProduct.Category.Name);

        Assert.Single(loadedProduct.Variants);
        Assert.Equal("200 mg", loadedProduct.Variants.First().Strength);

        Assert.Single(loadedProduct.Images);
        Assert.Equal("/images/product-one.jpg", loadedProduct.Images.First().ImageUrl);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenFound()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(product.Id);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Product One", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(99999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnProduct_WhenFound()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetBySlugAsync("product-one");

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Product One", result.Name);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetBySlugAsync("does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProduct()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        var savedProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Slug == "product-one");

        Assert.NotNull(savedProduct);
        Assert.Equal("Product One", savedProduct.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        product.Name = "Updated Product";

        await _repository.UpdateAsync(product);
        await _context.SaveChangesAsync();

        var updatedProduct = await _context.Products
            .AsNoTracking()
            .FirstAsync(p => p.Id == product.Id);

        Assert.Equal("Updated Product", updatedProduct.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(product);
        await _context.SaveChangesAsync();

        var deletedProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task ExistsBySlugAsync_ShouldReturnTrue_WhenSlugExists()
    {
        var category = new Category
        {
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        var product = CreateProduct("Product One", "product-one", category);

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsBySlugAsync("product-one");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsBySlugAsync_ShouldReturnFalse_WhenSlugDoesNotExist()
    {
        var result = await _repository.ExistsBySlugAsync("does-not-exist");

        Assert.False(result);
    }

    private static Product CreateProduct(
        string name,
        string slug,
        Category category)
    {
        return new Product
        {
            Name = name,
            Slug = slug,
            GenericName = "Amoxicillin",
            Composition = "Amoxicillin 500 mg",
            DosageForm = "Tablet",
            Description = "Test product",
            Manufacturer = "Rashe Lifesciences",
            Category = category,
            IsActive = true
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
