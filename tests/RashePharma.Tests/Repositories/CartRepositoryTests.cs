using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class CartRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly CartRepository _repository;

    public CartRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new CartRepository(_context);
    }

    // =========================================================
    // GetByUserIdAsync
    // =========================================================

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnCartWithItemsAndProductDetails()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var result = await _repository.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(10, result.UserId);

        Assert.Single(result.Items);

        var item = result.Items.First();
        Assert.Equal(1, item.Id);
        Assert.Equal(5, item.ProductVariantId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(125, item.UnitPrice);

        Assert.NotNull(item.ProductVariant);
        Assert.Equal("200 mg", item.ProductVariant!.Strength);
        Assert.Equal("10 Tablets", item.ProductVariant.PackSize);
        Assert.Equal(125, item.ProductVariant.Price);

        Assert.NotNull(item.ProductVariant.Product);
        Assert.Equal("MEDOFCIN-200", item.ProductVariant.Product!.Name);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenCartDoesNotExist()
    {
        var result = await _repository.GetByUserIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyRequestedUsersCart()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);
        await SeedUserAsync(20);

        var secondCart = new Cart
        {
            Id = 2,
            UserId = 20
        };

        _context.Carts.Add(secondCart);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result.UserId);
        Assert.Equal(1, result.Id);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCartWithItemsAndProductDetails()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(10, result.UserId);

        Assert.Single(result.Items);
        Assert.NotNull(result.Items.First().ProductVariant);
        Assert.NotNull(result.Items.First().ProductVariant!.Product);
        Assert.Equal(
            "MEDOFCIN-200",
            result.Items.First().ProductVariant!.Product!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCartDoesNotExist()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    // =========================================================
    // AddAsync
    // =========================================================

    [Fact]
    public async Task AddAsync_ShouldAddCart()
    {
        await SeedUserAsync(10);

        var cart = new Cart
        {
            Id = 1,
            UserId = 10
        };

        await _repository.AddAsync(cart);
        await _context.SaveChangesAsync();

        var savedCart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == 1);

        Assert.NotNull(savedCart);
        Assert.Equal(10, savedCart.UserId);
    }

    // =========================================================
    // UpdateAsync
    // =========================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCart()
    {
        await SeedUserAsync(10);

        var cart = new Cart
        {
            Id = 1,
            UserId = 10
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        cart.UserId = 10;

        await _repository.UpdateAsync(cart);
        await _context.SaveChangesAsync();

        var updatedCart = await _context.Carts
            .AsNoTracking()
            .FirstAsync(c => c.Id == 1);

        Assert.Equal(10, updatedCart.UserId);
    }

    // =========================================================
    // GetItemAsync
    // =========================================================

    [Fact]
    public async Task GetItemAsync_ShouldReturnItem_WhenItemExists()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var result = await _repository.GetItemAsync(1, 5);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.CartId);
        Assert.Equal(5, result.ProductVariantId);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(125, result.UnitPrice);
    }

    [Fact]
    public async Task GetItemAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var result = await _repository.GetItemAsync(1, 999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemAsync_ShouldNotReturnItemFromAnotherCart()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var result = await _repository.GetItemAsync(999, 5);

        Assert.Null(result);
    }

    // =========================================================
    // AddItemAsync
    // =========================================================

    [Fact]
    public async Task AddItemAsync_ShouldAddCartItem()
    {
        await SeedCartGraphAsync(
            userId: 10,
            cartId: 1,
            itemId: 1,
            variantId: 5,
            includeItem: false);

        var item = new CartItem
        {
            Id = 1,
            CartId = 1,
            ProductVariantId = 5,
            Quantity = 3,
            UnitPrice = 125
        };

        await _repository.AddItemAsync(item);
        await _context.SaveChangesAsync();

        var savedItem = await _context.CartItems
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == 1);

        Assert.NotNull(savedItem);
        Assert.Equal(1, savedItem.CartId);
        Assert.Equal(5, savedItem.ProductVariantId);
        Assert.Equal(3, savedItem.Quantity);
        Assert.Equal(125, savedItem.UnitPrice);
    }

    // =========================================================
    // UpdateItemAsync
    // =========================================================

    [Fact]
    public async Task UpdateItemAsync_ShouldUpdateCartItem()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var item = await _context.CartItems.FirstAsync(i => i.Id == 1);

        item.Quantity = 5;
        item.UnitPrice = 150;

        await _repository.UpdateItemAsync(item);
        await _context.SaveChangesAsync();

        var updatedItem = await _context.CartItems
            .AsNoTracking()
            .FirstAsync(i => i.Id == 1);

        Assert.Equal(5, updatedItem.Quantity);
        Assert.Equal(150, updatedItem.UnitPrice);
    }

    // =========================================================
    // RemoveItemAsync
    // =========================================================

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveCartItem()
    {
        await SeedCartGraphAsync(userId: 10, cartId: 1, itemId: 1, variantId: 5);

        var item = await _context.CartItems.FirstAsync(i => i.Id == 1);

        await _repository.RemoveItemAsync(item);
        await _context.SaveChangesAsync();

        var deletedItem = await _context.CartItems
            .FirstOrDefaultAsync(i => i.Id == 1);

        Assert.Null(deletedItem);
    }

    // =========================================================
    // Helper Methods
    // =========================================================

    private async Task SeedCartGraphAsync(
        int userId,
        int cartId,
        int itemId,
        int variantId,
        bool includeItem = true)
    {
        await SeedUserAsync(userId);

        var category = new Category
        {
            Id = 1,
            Name = "Antibiotics",
            Slug = "antibiotics"
        };

        _context.Categories.Add(category);

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Antibiotic tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        _context.Products.Add(product);

        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = 1,
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = 125,
            SKU = "MEDOFCIN-200-10",
            StockQuantity = 50,
            IsActive = true
        };

        _context.ProductVariants.Add(variant);

        var cart = new Cart
        {
            Id = cartId,
            UserId = userId
        };

        _context.Carts.Add(cart);

        if (includeItem)
        {
            var item = new CartItem
            {
                Id = itemId,
                CartId = cartId,
                ProductVariantId = variantId,
                Quantity = 2,
                UnitPrice = 125
            };

            _context.CartItems.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    private async Task SeedUserAsync(int userId)
    {
        if (await _context.Users.AnyAsync(u => u.Id == userId))
            return;

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == 2);

        if (role == null)
        {
            role = new Role
            {
                Id = 2,
                Name = "Customer"
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"test{userId}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            RoleId = role.Id,
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
