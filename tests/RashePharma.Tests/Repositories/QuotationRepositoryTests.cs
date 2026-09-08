using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class QuotationRepositoryTests
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

    private static async Task SeedBaseDataAsync(ApplicationDbContext context)
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
            Email = "quotation-test@example.com",
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

        var enquiry = new Enquiry
        {
            Id = 1,
            UserId = 1,
            EnquiryNumber = "ENQ-001",
            CustomerName = "Test Customer",
            Email = "customer@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            Message = "Need quotation",
            Status = "Pending",
            CreatedAt = new DateTime(2026, 1, 1)
        };

        context.Roles.Add(role);
        context.Users.Add(user);
        context.Categories.Add(category);
        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.Enquiries.Add(enquiry);

        await context.SaveChangesAsync();
    }

    private static Quotation CreateQuotation(
        int id,
        int enquiryId,
        int? userId,
        string quoteNumber,
        DateTime createdAt)
    {
        return new Quotation
        {
            Id = id,
            EnquiryId = enquiryId,
            UserId = userId,
            QuoteNumber = quoteNumber,
            TotalAmount = 500m,
            Currency = "USD",
            Status = "Draft",
            ValidUntil = new DateTime(2026, 12, 31),
            Notes = "Test quotation",
            CreatedAt = createdAt
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllQuotations_OrderedByCreatedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var older = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            var newer = CreateQuotation(
                2, 1, 1, "QUO-002", new DateTime(2026, 2, 1));

            context.Quotations.AddRange(older, newer);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("QUO-002", result[0].QuoteNumber);
            Assert.Equal("QUO-001", result[1].QuoteNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoQuotationsExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new QuotationRepository(context);

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
    public async Task GetAllAsync_ShouldIncludeEnquiry()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Quotations.Add(CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetAllAsync();

            var quotation = Assert.Single(result);

            Assert.NotNull(quotation.Enquiry);
            Assert.Equal("ENQ-001", quotation.Enquiry.EnquiryNumber);
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
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            quotation.Items.Add(new QuotationItem
            {
                Id = 1,
                QuotationId = 1,
                ProductVariantId = 1,
                Quantity = 5,
                UnitPrice = 95m,
                TotalPrice = 475m
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetAllAsync();

            var item = Assert.Single(Assert.Single(result).Items);

            Assert.Equal(5, item.Quantity);
            Assert.Equal(95m, item.UnitPrice);
            Assert.Equal(475m, item.TotalPrice);
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
    public async Task GetByUserIdAsync_ShouldReturnOnlyQuotationsForUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var otherUser = new User
            {
                Id = 2,
                FirstName = "Other",
                LastName = "User",
                Email = "other-quotation-test@example.com",
                PhoneNumber = "+91-9876543211",
                Country = "India",
                PasswordHash = "test-password-hash",
                RoleId = 2,
                IsActive = true
            };

            var secondEnquiry = new Enquiry
            {
                Id = 2,
                UserId = 2,
                EnquiryNumber = "ENQ-002",
                CustomerName = "Other Customer",
                Email = "other-customer@example.com",
                Country = "India",
                Status = "Pending",
                CreatedAt = new DateTime(2026, 1, 2)
            };

            context.Users.Add(otherUser);
            context.Enquiries.Add(secondEnquiry);
            await context.SaveChangesAsync();

            context.Quotations.AddRange(
                CreateQuotation(1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)),
                CreateQuotation(2, 2, 2, "QUO-002", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var quotation = Assert.Single(result);

            Assert.Equal("QUO-001", quotation.QuoteNumber);
            Assert.Equal(1, quotation.UserId);
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
            await SeedBaseDataAsync(context);

            context.Quotations.AddRange(
                CreateQuotation(1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)),
                CreateQuotation(2, 1, 1, "QUO-002", new DateTime(2026, 3, 1)),
                CreateQuotation(3, 1, 1, "QUO-003", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.Equal(3, result.Count);
            Assert.Equal("QUO-002", result[0].QuoteNumber);
            Assert.Equal("QUO-003", result[1].QuoteNumber);
            Assert.Equal("QUO-001", result[2].QuoteNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldIncludeEnquiryAndProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            quotation.Items.Add(new QuotationItem
            {
                Id = 1,
                QuotationId = 1,
                ProductVariantId = 1,
                Quantity = 2,
                UnitPrice = 90m,
                TotalPrice = 180m
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var savedQuotation = Assert.Single(result);
            var item = Assert.Single(savedQuotation.Items);

            Assert.NotNull(savedQuotation.Enquiry);
            Assert.Equal("ENQ-001", savedQuotation.Enquiry.EnquiryNumber);
            Assert.NotNull(item.ProductVariant);
            Assert.NotNull(item.ProductVariant.Product);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldReturnOnlyQuotationsForEnquiry()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var secondEnquiry = new Enquiry
            {
                Id = 2,
                UserId = 1,
                EnquiryNumber = "ENQ-002",
                CustomerName = "Test Customer 2",
                Email = "customer2@example.com",
                Country = "India",
                Status = "Pending",
                CreatedAt = new DateTime(2026, 1, 2)
            };

            context.Enquiries.Add(secondEnquiry);
            await context.SaveChangesAsync();

            context.Quotations.AddRange(
                CreateQuotation(1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)),
                CreateQuotation(2, 2, 1, "QUO-002", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByEnquiryIdAsync(1);

            var quotation = Assert.Single(result);

            Assert.Equal("QUO-001", quotation.QuoteNumber);
            Assert.Equal(1, quotation.EnquiryId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldOrderResultsByCreatedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Quotations.AddRange(
                CreateQuotation(1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)),
                CreateQuotation(2, 1, 1, "QUO-002", new DateTime(2026, 3, 1)),
                CreateQuotation(3, 1, 1, "QUO-003", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByEnquiryIdAsync(1);

            Assert.Equal(3, result.Count);
            Assert.Equal("QUO-002", result[0].QuoteNumber);
            Assert.Equal("QUO-003", result[1].QuoteNumber);
            Assert.Equal("QUO-001", result[2].QuoteNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldIncludeProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            quotation.Items.Add(new QuotationItem
            {
                Id = 1,
                QuotationId = 1,
                ProductVariantId = 1,
                Quantity = 4,
                UnitPrice = 85m,
                TotalPrice = 340m
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByEnquiryIdAsync(1);

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
    public async Task GetByIdAsync_ShouldReturnQuotation_WhenIdExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Quotations.Add(CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("QUO-001", result.QuoteNumber);
            Assert.Equal(500m, result.TotalAmount);
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
            var repository = new QuotationRepository(context);

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
    public async Task GetByIdAsync_ShouldIncludeEnquiryAndItems()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            quotation.Items.Add(new QuotationItem
            {
                Id = 1,
                QuotationId = 1,
                ProductVariantId = 1,
                Quantity = 3,
                UnitPrice = 100m,
                TotalPrice = 300m
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.Enquiry);
            Assert.Single(result.Items);
            Assert.NotNull(result.Items.Single().ProductVariant);
            Assert.NotNull(result.Items.Single().ProductVariant.Product);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnQuotation_WhenNumberExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Quotations.Add(CreateQuotation(
                1, 1, 1, "QUO-ABC-123", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByQuoteNumberAsync("QUO-ABC-123");

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("QUO-ABC-123", result.QuoteNumber);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnNull_WhenNumberDoesNotExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new QuotationRepository(context);

            var result = await repository.GetByQuoteNumberAsync("DOES-NOT-EXIST");

            Assert.Null(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldIncludeEnquiryAndProductGraph()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            quotation.Items.Add(new QuotationItem
            {
                Id = 1,
                QuotationId = 1,
                ProductVariantId = 1,
                Quantity = 6,
                UnitPrice = 80m,
                TotalPrice = 480m
            });

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            var repository = new QuotationRepository(context);

            var result = await repository.GetByQuoteNumberAsync("QUO-001");

            Assert.NotNull(result);
            Assert.NotNull(result.Enquiry);

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
    public async Task AddAsync_ShouldAddQuotationToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-NEW-001", new DateTime(2026, 4, 1));

            var repository = new QuotationRepository(context);

            await repository.AddAsync(quotation);
            await context.SaveChangesAsync();

            var saved = await context.Quotations
                .FirstOrDefaultAsync(q => q.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal("QUO-NEW-001", saved.QuoteNumber);
            Assert.Equal(500m, saved.TotalAmount);
            Assert.Equal("USD", saved.Currency);
            Assert.Equal("Draft", saved.Status);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingQuotation()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var quotation = CreateQuotation(
                1, 1, 1, "QUO-001", new DateTime(2026, 1, 1));

            context.Quotations.Add(quotation);
            await context.SaveChangesAsync();

            quotation.Status = "Sent";
            quotation.TotalAmount = 750m;
            quotation.Notes = "Updated quotation";
            quotation.UpdatedAt = new DateTime(2026, 4, 1);

            var repository = new QuotationRepository(context);

            await repository.UpdateAsync(quotation);
            await context.SaveChangesAsync();

            var updated = await context.Quotations
                .AsNoTracking()
                .FirstAsync(q => q.Id == 1);

            Assert.Equal("Sent", updated.Status);
            Assert.Equal(750m, updated.TotalAmount);
            Assert.Equal("Updated quotation", updated.Notes);
            Assert.Equal(new DateTime(2026, 4, 1), updated.UpdatedAt);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
