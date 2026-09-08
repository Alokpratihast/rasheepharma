using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class PartnerRepositoryTests
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
        context.Roles.Add(new Role { Id = 2, Name = "Customer" });

        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "partner-test@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            RoleId = 2,
            IsActive = true
        });

        context.PartnerRequests.Add(new PartnerRequest
        {
            Id = 1,
            UserId = 1,
            CompanyName = "Test Pharma Pvt Ltd",
            ContactPerson = "Test Contact",
            Email = "partner-request@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "1000 units",
            Message = "Test partner request",
            Status = "Approved",
            CreatedAt = new DateTime(2026, 1, 1)
        });

        await context.SaveChangesAsync();
    }

    private static Partner CreatePartner(
        int id,
        int? userId,
        int? partnerRequestId,
        string companyName,
        DateTime joinedAt)
    {
        return new Partner
        {
            Id = id,
            UserId = userId,
            PartnerRequestId = partnerRequestId,
            CompanyName = companyName,
            ContactPerson = "Test Contact",
            Email = $"{companyName.Replace(" ", "").ToLowerInvariant()}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            RegistrationNumber = "REG-001",
            TaxIdentificationNumber = "TAX-001",
            Status = "Active",
            JoinedAt = joinedAt
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPartners_OrderedByJoinedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.PartnerRequests.Add(new PartnerRequest
            {
                Id = 2,
                CompanyName = "Second Request",
                ContactPerson = "Second Contact",
                Email = "second-request@example.com",
                Country = "India",
                Status = "Approved"
            });

            await context.SaveChangesAsync();

            context.Partners.AddRange(
                CreatePartner(1, 1, 1, "Older Pharma", new DateTime(2026, 1, 1)),
                CreatePartner(2, 1, 2, "Newer Pharma", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Newer Pharma", result[0].CompanyName);
            Assert.Equal("Older Pharma", result[1].CompanyName);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoPartnersExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new PartnerRepository(context);

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
    public async Task GetAllAsync_ShouldIncludeUserAndPartnerRequest()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Partners.Add(CreatePartner(
                1, 1, 1, "Test Pharma Pvt Ltd", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetAllAsync();

            var partner = Assert.Single(result);

            Assert.NotNull(partner.User);
            Assert.Equal("partner-test@example.com", partner.User.Email);

            Assert.NotNull(partner.PartnerRequest);
            Assert.Equal("Test Pharma Pvt Ltd", partner.PartnerRequest.CompanyName);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnPartner_WhenIdExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Partners.Add(CreatePartner(
                1, 1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Pharma", result.CompanyName);
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
            var repository = new PartnerRepository(context);

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
    public async Task GetByIdAsync_ShouldIncludeUserAndPartnerRequest()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Partners.Add(CreatePartner(
                1, 1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.NotNull(result.PartnerRequest);
            Assert.Equal("partner-test@example.com", result.User.Email);
            Assert.Equal("Approved", result.PartnerRequest.Status);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnPartner_WhenUserIdMatches()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Partners.Add(CreatePartner(
                1, 1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test Pharma", result.CompanyName);
            Assert.Equal(1, result.UserId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnNull_WhenUserHasNoPartner()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var repository = new PartnerRepository(context);

            var result = await repository.GetByUserIdAsync(999);

            Assert.Null(result);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldIncludePartnerRequest()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            context.Partners.Add(CreatePartner(
                1, 1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.PartnerRequest);
            Assert.Equal(1, result.PartnerRequest.Id);
            Assert.Equal("Approved", result.PartnerRequest.Status);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddPartnerToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var partner = CreatePartner(
                1, 1, 1, "New Partner Pharma", new DateTime(2026, 4, 1));

            var repository = new PartnerRepository(context);

            await repository.AddAsync(partner);
            await context.SaveChangesAsync();

            var saved = await context.Partners
                .FirstOrDefaultAsync(p => p.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal("New Partner Pharma", saved.CompanyName);
            Assert.Equal("Active", saved.Status);
            Assert.Equal(1, saved.UserId);
            Assert.Equal(1, saved.PartnerRequestId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingPartner()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedBaseDataAsync(context);

            var partner = CreatePartner(
                1, 1, 1, "Original Pharma", new DateTime(2026, 1, 1));

            context.Partners.Add(partner);
            await context.SaveChangesAsync();

            partner.CompanyName = "Updated Pharma";
            partner.Status = "Inactive";
            partner.BusinessType = "Wholesaler";
            partner.UpdatedAt = new DateTime(2026, 4, 1);

            var repository = new PartnerRepository(context);

            await repository.UpdateAsync(partner);
            await context.SaveChangesAsync();

            var updated = await context.Partners
                .AsNoTracking()
                .FirstAsync(p => p.Id == 1);

            Assert.Equal("Updated Pharma", updated.CompanyName);
            Assert.Equal("Inactive", updated.Status);
            Assert.Equal("Wholesaler", updated.BusinessType);
            Assert.Equal(new DateTime(2026, 4, 1), updated.UpdatedAt);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
