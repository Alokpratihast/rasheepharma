using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;

namespace RashePharma.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new UserRepository(_context);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenFound()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldLoadRole_WhenUserFound()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.NotNull(result.Role);
        Assert.Equal("Customer", result.Role.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(99999);

        Assert.Null(result);
    }

    // =========================================================
    // GetByEmailAsync
    // =========================================================

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenFound()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync(
            "john@example.com");

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldLoadRole_WhenUserFound()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEmailAsync(
            "john@example.com");

        Assert.NotNull(result);
        Assert.NotNull(result.Role);
        Assert.Equal("Customer", result.Role.Name);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByEmailAsync(
            "notfound@example.com");

        Assert.Null(result);
    }

    // =========================================================
    // AddAsync
    // =========================================================

    [Fact]
    public async Task AddAsync_ShouldAddUser()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);

        await _repository.AddAsync(user);
        await _context.SaveChangesAsync();

        var savedUser = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.Email == "john@example.com");

        Assert.NotNull(savedUser);
        Assert.Equal("John", savedUser.FirstName);
        Assert.Equal("Doe", savedUser.LastName);
        Assert.Equal("john@example.com", savedUser.Email);
    }

    // =========================================================
    // UpdateAsync
    // =========================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        user.FirstName = "Updated";
        user.LastName = "User";

        await _repository.UpdateAsync(user);
        await _context.SaveChangesAsync();

        var updatedUser = await _context.Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == user.Id);

        Assert.Equal("Updated", updatedUser.FirstName);
        Assert.Equal("User", updatedUser.LastName);
    }

    // =========================================================
    // ExistsByEmailAsync
    // =========================================================

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenEmailExists()
    {
        var role = CreateRole();
        var user = CreateUser(role);

        _context.Roles.Add(role);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByEmailAsync(
            "john@example.com");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnFalse_WhenEmailDoesNotExist()
    {
        var result = await _repository.ExistsByEmailAsync(
            "notfound@example.com");

        Assert.False(result);
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static Role CreateRole()
    {
        return new Role
        {
            Name = "Customer"
        };
    }

    private static User CreateUser(Role role)
    {
        return new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            IsActive = true,
            Role = role
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