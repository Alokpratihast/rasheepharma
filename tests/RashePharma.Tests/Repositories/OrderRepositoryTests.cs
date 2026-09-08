using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class OrderRepositoryTests
{
    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<(ApplicationDbContext Context, SqliteConnection Connection)> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        return (context, connection);
    }

    private static async Task SeedProductGraphAsync(ApplicationDbContext context)
    {
        var role = new Role
        {
            Id = 2,
            Name = "Customer"
        };

        var user = new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "order-test@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            RoleId = 2,
            IsActive = true
        };

        var category = new Category
        {
            Id = 1,
            Name = "Antibiotics",
            Slug = "antibiotics",
            IsActive = true
        };

        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Slug = "test-product",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        var variant = new ProductVariant
        {
            Id = 1,
            ProductId = 1,
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = 100m,
            SKU = "TEST-200",
            StockQuantity = 50,
            IsActive = true
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        context.Categories.Add(category);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);

        await context.SaveChangesAsync();
    }

    private static Order CreateOrder(
        int id,
        int userId,
        string orderNumber,
        DateTime createdAt)
    {
        return new Order
        {
            Id = id,
            UserId = userId,
            OrderNumber = orderNumber,
            CreatedAt = createdAt,
            Status = "Pending",
            TotalAmount = 200m
        };
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyOrdersForUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var olderOrder = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            var newerOrder = CreateOrder(
                2,
                1,
                "ORD-002",
                new DateTime(2026, 2, 1));

            var otherUserOrder = CreateOrder(
                3,
                999,
                "ORD-003",
                new DateTime(2026, 3, 1));

            // Only the existing user can satisfy the FK, so disable FK temporarily
            // is not appropriate here. Instead create another valid user.
            var otherUser = new User
            {
                Id = 2,
                FirstName = "Other",
                LastName = "User",
                Email = "other-order-test@example.com",
                PhoneNumber = "+91-9876543211",
                Country = "India",
                PasswordHash = "test-password-hash",
                RoleId = 2,
                IsActive = true
            };

            context.Users.Add(otherUser);
            await context.SaveChangesAsync();

            otherUserOrder.UserId = 2;

            context.Orders.AddRange(olderOrder, newerOrder, otherUserOrder);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.Equal(2, result.Count);
            Assert.Equal("ORD-002", result[0].OrderNumber);
            Assert.Equal("ORD-001", result[1].OrderNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoOrders()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var repository = new OrderRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.Empty(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldIncludeItemsProductVariantAndProduct()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            order.Items.Add(new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductVariantId = 1,
                Quantity = 2,
                UnitPrice = 100m,
                
            });

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var item = Assert.Single(result).Items.Single();

            Assert.NotNull(item.ProductVariant);
            Assert.NotNull(item.ProductVariant.Product);
            Assert.Equal("Test Product", item.ProductVariant.Product.Name);
            Assert.Equal("TEST-200", item.ProductVariant.SKU);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldIncludeStatusHistory()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = 1,
                OrderId = 1,
                Status = "Pending",
                CreatedAt = new DateTime(2026, 1, 1)
            });

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var history = Assert.Single(Assert.Single(result).StatusHistory);

            Assert.Equal("Pending", history.Status);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenIdExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            context.Orders.Add(CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("ORD-001", result.OrderNumber);
            Assert.Equal(1, result.UserId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenIdDoesNotExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var repository = new OrderRepository(context);

            var result = await repository.GetByIdAsync(999);

            Assert.Null(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeItemsAndStatusHistory()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            order.Items.Add(new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductVariantId = 1,
                Quantity = 1,
                UnitPrice = 100m,
                
            });

            order.StatusHistory.Add(new OrderStatusHistory
            {
                Id = 1,
                OrderId = 1,
                Status = "Confirmed",
                CreatedAt = new DateTime(2026, 1, 2)
            });

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Single(result.StatusHistory);
            Assert.NotNull(result.Items.First().ProductVariant);
            Assert.NotNull(result.Items.First().ProductVariant.Product);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnOrder_WhenOrderNumberExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            context.Orders.Add(CreateOrder(
                1,
                1,
                "ORD-ABC-123",
                new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByOrderNumberAsync("ORD-ABC-123");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ORD-ABC-123", result.OrderNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnNull_WhenOrderNumberDoesNotExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var repository = new OrderRepository(context);

            var result = await repository.GetByOrderNumberAsync("DOES-NOT-EXIST");

            Assert.Null(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldIncludeItemsAndProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            order.Items.Add(new OrderItem
            {
                Id = 1,
                OrderId = 1,
                ProductVariantId = 1,
                Quantity = 3,
                UnitPrice = 100m,
                
            });

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var repository = new OrderRepository(context);

            var result = await repository.GetByOrderNumberAsync("ORD-001");

            Assert.NotNull(result);

            var item = Assert.Single(result.Items);

            Assert.NotNull(item.ProductVariant);
            Assert.NotNull(item.ProductVariant.Product);
            Assert.Equal("Test Product", item.ProductVariant.Product.Name);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddOrderToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-NEW-001",
                new DateTime(2026, 4, 1));

            var repository = new OrderRepository(context);

            await repository.AddAsync(order);
            await context.SaveChangesAsync();

            var saved = await context.Orders.FirstOrDefaultAsync(o => o.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal("ORD-NEW-001", saved.OrderNumber);
            Assert.Equal(200m, saved.TotalAmount);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingOrder()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            order.Status = "Confirmed";
            order.TotalAmount = 250m;

            var repository = new OrderRepository(context);

            await repository.UpdateAsync(order);
            await context.SaveChangesAsync();

            var updated = await context.Orders.AsNoTracking()
                .FirstAsync(o => o.Id == 1);

            Assert.Equal("Confirmed", updated.Status);
            Assert.Equal(250m, updated.TotalAmount);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddStatusHistoryAsync_ShouldAddHistoryToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var order = CreateOrder(
                1,
                1,
                "ORD-001",
                new DateTime(2026, 1, 1));

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var history = new OrderStatusHistory
            {
                Id = 1,
                OrderId = 1,
                Status = "Shipped",
                CreatedAt = new DateTime(2026, 1, 3)
            };

            var repository = new OrderRepository(context);

            await repository.AddStatusHistoryAsync(history);
            await context.SaveChangesAsync();

            var saved = await context.OrderStatusHistories
                .FirstOrDefaultAsync(h => h.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal(1, saved.OrderId);
            Assert.Equal("Shipped", saved.Status);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
