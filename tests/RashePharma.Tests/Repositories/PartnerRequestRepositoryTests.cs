using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class PartnerRequestRepositoryTests
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

    private static async Task SeedUserAsync(ApplicationDbContext context)
    {
        context.Roles.Add(new Role
        {
            Id = 2,
            Name = "Customer"
        });

        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "partner-request-test@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            RoleId = 2,
            IsActive = true
        });

        await context.SaveChangesAsync();
    }

    private static PartnerRequest CreateRequest(
        int id,
        int? userId,
        string companyName,
        DateTime createdAt)
    {
        return new PartnerRequest
        {
            Id = id,
            UserId = userId,
            CompanyName = companyName,
            ContactPerson = "Test Contact",
            Email = $"{companyName.Replace(" ", "").ToLowerInvariant()}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "1000 units",
            Message = "Test partner request",
            Status = "Pending",
            CreatedAt = createdAt
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRequests_OrderedByCreatedAtDescending()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            context.PartnerRequests.AddRange(
                CreateRequest(1, 1, "Older Pharma", new DateTime(2026, 1, 1)),
                CreateRequest(2, 1, "Newer Pharma", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

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
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoRequestsExist()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            var repository = new PartnerRequestRepository(context);

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
    public async Task GetAllAsync_ShouldIncludeUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            context.PartnerRequests.Add(CreateRequest(
                1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

            var result = await repository.GetAllAsync();

            var request = Assert.Single(result);

            Assert.NotNull(request.User);
            Assert.Equal("partner-request-test@example.com", request.User.Email);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRequest_WhenIdExists()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            context.PartnerRequests.Add(CreateRequest(
                1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

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
            var repository = new PartnerRequestRepository(context);

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
    public async Task GetByIdAsync_ShouldIncludeUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            context.PartnerRequests.Add(CreateRequest(
                1, 1, "Test Pharma", new DateTime(2026, 1, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

            var result = await repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal("Test", result.User.FirstName);
            Assert.Equal("User", result.User.LastName);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyRequestsForUser()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            var otherUser = new User
            {
                Id = 2,
                FirstName = "Other",
                LastName = "User",
                Email = "other-partner-request@example.com",
                PhoneNumber = "+91-9876543211",
                Country = "India",
                PasswordHash = "test-password-hash",
                RoleId = 2,
                IsActive = true
            };

            context.Users.Add(otherUser);
            await context.SaveChangesAsync();

            context.PartnerRequests.AddRange(
                CreateRequest(1, 1, "My Pharma", new DateTime(2026, 1, 1)),
                CreateRequest(2, 2, "Other Pharma", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            var request = Assert.Single(result);

            Assert.Equal("My Pharma", request.CompanyName);
            Assert.Equal(1, request.UserId);
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
            await SeedUserAsync(context);

            context.PartnerRequests.AddRange(
                CreateRequest(1, 1, "Older Pharma", new DateTime(2026, 1, 1)),
                CreateRequest(2, 1, "Newer Pharma", new DateTime(2026, 3, 1)),
                CreateRequest(3, 1, "Middle Pharma", new DateTime(2026, 2, 1)));

            await context.SaveChangesAsync();

            var repository = new PartnerRequestRepository(context);

            var result = await repository.GetByUserIdAsync(1);

            Assert.Equal(3, result.Count);
            Assert.Equal("Newer Pharma", result[0].CompanyName);
            Assert.Equal("Middle Pharma", result[1].CompanyName);
            Assert.Equal("Older Pharma", result[2].CompanyName);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoRequests()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            var repository = new PartnerRequestRepository(context);

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
    public async Task AddAsync_ShouldAddPartnerRequestToDatabase()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            var request = CreateRequest(
                1, 1, "New Partner Pharma", new DateTime(2026, 4, 1));

            var repository = new PartnerRequestRepository(context);

            await repository.AddAsync(request);
            await context.SaveChangesAsync();

            var saved = await context.PartnerRequests
                .FirstOrDefaultAsync(pr => pr.Id == 1);

            Assert.NotNull(saved);
            Assert.Equal("New Partner Pharma", saved.CompanyName);
            Assert.Equal("Pending", saved.Status);
            Assert.Equal(1, saved.UserId);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingPartnerRequest()
    {
        var (context, connection) = await CreateDatabaseAsync();

        try
        {
            await SeedUserAsync(context);

            var request = CreateRequest(
                1, 1, "Original Pharma", new DateTime(2026, 1, 1));

            context.PartnerRequests.Add(request);
            await context.SaveChangesAsync();

            request.CompanyName = "Updated Pharma";
            request.Status = "Approved";
            request.ExpectedVolume = "5000 units";
            request.Message = "Approved for partnership.";
            request.UpdatedAt = new DateTime(2026, 4, 1);

            var repository = new PartnerRequestRepository(context);

            await repository.UpdateAsync(request);
            await context.SaveChangesAsync();

            var updated = await context.PartnerRequests
                .AsNoTracking()
                .FirstAsync(pr => pr.Id == 1);

            Assert.Equal("Updated Pharma", updated.CompanyName);
            Assert.Equal("Approved", updated.Status);
            Assert.Equal("5000 units", updated.ExpectedVolume);
            Assert.Equal("Approved for partnership.", updated.Message);
            Assert.Equal(new DateTime(2026, 4, 1), updated.UpdatedAt);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
