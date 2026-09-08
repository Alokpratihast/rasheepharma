using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class AddressRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly AddressRepository _repository;

    public AddressRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new AddressRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserAddresses()
    {
        await SeedUserAsync(10);

        var address1 = CreateAddress(10, "Address One");
        var address2 = CreateAddress(10, "Address Two");
        var otherUserAddress = CreateAddress(20, "Other User Address");

        await SeedUserAsync(20);

        _context.Addresses.AddRange(
            address1,
            address2,
            otherUserAddress);

        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(10);

        Assert.Equal(2, result.Count);
        Assert.All(result, address => Assert.Equal(10, address.UserId));
        Assert.Contains(result, address => address.AddressLine1 == "Address One");
        Assert.Contains(result, address => address.AddressLine1 == "Address Two");
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoAddresses()
    {
        await SeedUserAsync(10);

        var address = CreateAddress(10, "User Address");

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(999);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAddress_WhenFound()
    {
        await SeedUserAsync(10);

        var address = CreateAddress(10, "123 Main Street");

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(address.Id);

        Assert.NotNull(result);
        Assert.Equal(address.Id, result.Id);
        Assert.Equal(10, result.UserId);
        Assert.Equal("123 Main Street", result.AddressLine1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(99999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddAddress()
    {
        await SeedUserAsync(10);

        var address = CreateAddress(10, "123 Main Street");

        await _repository.AddAsync(address);
        await _context.SaveChangesAsync();

        var savedAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.AddressLine1 == "123 Main Street");

        Assert.NotNull(savedAddress);
        Assert.Equal(10, savedAddress.UserId);
        Assert.Equal("123 Main Street", savedAddress.AddressLine1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAddress()
    {
        await SeedUserAsync(10);

        var address = CreateAddress(10, "Old Address");

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        address.AddressLine1 = "Updated Address";
        address.City = "Bengaluru";

        await _repository.UpdateAsync(address);
        await _context.SaveChangesAsync();

        var updatedAddress = await _context.Addresses
            .AsNoTracking()
            .FirstAsync(a => a.Id == address.Id);

        Assert.Equal("Updated Address", updatedAddress.AddressLine1);
        Assert.Equal("Bengaluru", updatedAddress.City);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAddress()
    {
        await SeedUserAsync(10);

        var address = CreateAddress(10, "Address To Delete");

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        await _repository.DeleteAsync(address);
        await _context.SaveChangesAsync();

        var deletedAddress = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == address.Id);

        Assert.Null(deletedAddress);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldNotReturnOtherUsersAddresses()
    {
        await SeedUserAsync(10);
        await SeedUserAsync(20);

        var user10Address = CreateAddress(10, "User 10 Address");
        var user20Address = CreateAddress(20, "User 20 Address");

        _context.Addresses.AddRange(user10Address, user20Address);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByUserIdAsync(10);

        Assert.Single(result);
        Assert.Equal("User 10 Address", result[0].AddressLine1);
        Assert.DoesNotContain(result, a => a.UserId == 20);
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

    private static Address CreateAddress(
        int userId,
        string addressLine1)
    {
        return new Address
        {
            UserId = userId,
            AddressLine1 = addressLine1,
            AddressLine2 = "Near Main Road",
            City = "Delhi",
            State = "Delhi",
            PostalCode = "110001",
            Country = "India",
            AddressType = "Shipping",
            IsDefault = false
        };
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
