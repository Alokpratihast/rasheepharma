using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class EnquiryRepositoryTests
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
            Email = "enquiry-test@example.com",
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

    private static Enquiry CreateEnquiry(
        int id,
        int userId,
        string enquiryNumber,
        DateTime createdAt)
    {
        return new Enquiry
        {
            Id = id,
            UserId = userId,
            EnquiryNumber = enquiryNumber,
            CustomerName = "Test Customer",
            Email = "customer@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            Message = "Test enquiry",
            Status = "Pending",
            CreatedAt = createdAt
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEnquiries_OrderedByCreatedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var older = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            var newer = CreateEnquiry(
                2,
                1,
                "ENQ-002",
                new DateTime(2026, 2, 1));

            context.Enquiries.AddRange(older, newer);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("ENQ-002", result[0].EnquiryNumber);
            Assert.Equal("ENQ-001", result[1].EnquiryNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoEnquiriesExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new EnquiryRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Empty(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeItemsProductVariantAndProduct()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            enquiry.Items.Add(new EnquiryItem
            {
                Id = 1,
                EnquiryId = 1,
                ProductVariantId = 1,
                Quantity = 5,
                Message = "Need quotation"
            });

            context.Enquiries.Add(enquiry);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetAllAsync();

            var item = Assert.Single(Assert.Single(result).Items);

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
    public async Task GetByUserIdAsync_ShouldReturnOnlyEnquiriesForUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var otherUser = new User
            {
                Id = 2,
                FirstName = "Other",
                LastName = "User",
                Email = "other-enquiry-test@example.com",
                PhoneNumber = "+91-9876543211",
                Country = "India",
                PasswordHash = "test-password-hash",
                RoleId = 2,
                IsActive = true
            };

            context.Users.Add(otherUser);
            await context.SaveChangesAsync();

            var userEnquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            var otherEnquiry = CreateEnquiry(
                2,
                2,
                "ENQ-002",
                new DateTime(2026, 2, 1));

            context.Enquiries.AddRange(userEnquiry, otherEnquiry);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var enquiry = Assert.Single(result);

            Assert.Equal("ENQ-001", enquiry.EnquiryNumber);
            Assert.Equal(1, enquiry.UserId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldOrderResultsByCreatedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var older = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            var newer = CreateEnquiry(
                2,
                1,
                "ENQ-002",
                new DateTime(2026, 3, 1));

            var middle = CreateEnquiry(
                3,
                1,
                "ENQ-003",
                new DateTime(2026, 2, 1));

            context.Enquiries.AddRange(older, newer, middle);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.Equal(3, result.Count);
            Assert.Equal("ENQ-002", result[0].EnquiryNumber);
            Assert.Equal("ENQ-003", result[1].EnquiryNumber);
            Assert.Equal("ENQ-001", result[2].EnquiryNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldIncludeProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            enquiry.Items.Add(new EnquiryItem
            {
                Id = 1,
                EnquiryId = 1,
                ProductVariantId = 1,
                Quantity = 2,
                Message = "Product requirement"
            });

            context.Enquiries.Add(enquiry);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var item = Assert.Single(Assert.Single(result).Items);

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
    public async Task GetByIdAsync_ShouldReturnEnquiry_WhenIdExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            context.Enquiries.Add(CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ENQ-001", result.EnquiryNumber);
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
            var repository = new EnquiryRepository(context);

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
    public async Task GetByIdAsync_ShouldIncludeItemsAndProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            enquiry.Items.Add(new EnquiryItem
            {
                Id = 1,
                EnquiryId = 1,
                ProductVariantId = 1,
                Quantity = 3,
                Message = "Need bulk quantity"
            });

            context.Enquiries.Add(enquiry);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);

            var item = Assert.Single(result.Items);

            Assert.Equal(3, item.Quantity);
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
    public async Task GetByEnquiryNumberAsync_ShouldReturnEnquiry_WhenNumberExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            context.Enquiries.Add(CreateEnquiry(
                1,
                1,
                "ENQ-ABC-123",
                new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByEnquiryNumberAsync("ENQ-ABC-123");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("ENQ-ABC-123", result.EnquiryNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByEnquiryNumberAsync_ShouldReturnNull_WhenNumberDoesNotExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new EnquiryRepository(context);

            var result = await repository.GetByEnquiryNumberAsync("DOES-NOT-EXIST");

            Assert.Null(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByEnquiryNumberAsync_ShouldIncludeProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            enquiry.Items.Add(new EnquiryItem
            {
                Id = 1,
                EnquiryId = 1,
                ProductVariantId = 1,
                Quantity = 4,
                Message = "Need product details"
            });

            context.Enquiries.Add(enquiry);
            await context.SaveChangesAsync();

            var repository = new EnquiryRepository(context);

            var result = await repository.GetByEnquiryNumberAsync("ENQ-001");

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
    public async Task AddAsync_ShouldAddEnquiryToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-NEW-001",
                new DateTime(2026, 4, 1));

            var repository = new EnquiryRepository(context);

            await repository.AddAsync(enquiry);
            await context.SaveChangesAsync();

            var saved = await context.Enquiries
                .FirstOrDefaultAsync(e => e.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal("ENQ-NEW-001", saved.EnquiryNumber);
            Assert.Equal("Pending", saved.Status);
            Assert.Equal("customer@example.com", saved.Email);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingEnquiry()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedProductGraphAsync(context);

            var enquiry = CreateEnquiry(
                1,
                1,
                "ENQ-001",
                new DateTime(2026, 1, 1));

            context.Enquiries.Add(enquiry);
            await context.SaveChangesAsync();

            enquiry.Status = "Responded";
            enquiry.Message = "We have responded to your enquiry.";

            var repository = new EnquiryRepository(context);

            await repository.UpdateAsync(enquiry);
            await context.SaveChangesAsync();

            var updated = await context.Enquiries
                .AsNoTracking()
                .FirstAsync(e => e.Id == 1);

            Assert.Equal("Responded", updated.Status);
            Assert.Equal(
                "We have responded to your enquiry.",
                updated.Message);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
