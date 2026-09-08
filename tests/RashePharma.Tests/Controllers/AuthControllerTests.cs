using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;
using Xunit;

namespace RashePharma.Tests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }


    // =========================================================
    // Test Data Helpers
    // =========================================================

    private async Task SeedRoleAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        if (!db.Roles.Any(r => r.Id == 2))
        {
            db.Roles.Add(new Role
            {
                Id = 2,
                Name = "Customer"
            });

            await db.SaveChangesAsync();
        }
    }


    private async Task<int> CreateUserAsync()
    {
        await SeedRoleAsync();

        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = $"auth-test-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "password123",
            RoleId = 2,
            IsActive = true
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();

        return user.Id;
    }


    private async Task<string> RegisterAndLoginAsync()
    {
        await SeedRoleAsync();

        var email =
            $"profile-{Guid.NewGuid():N}@example.com";

        var registerDto = new RegisterDto
        {
            FirstName = "Profile",
            LastName = "User",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));

        return loginResult.Token;
    }


    // =========================================================
    // REGISTER
    // =========================================================

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        await SeedRoleAsync();

        var dto = new RegisterDto
        {
            FirstName = "New",
            LastName = "User",
            Email = $"register-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }


    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
    {
        // Arrange
        await SeedRoleAsync();

        var email =
            $"duplicate-{Guid.NewGuid():N}@example.com";

        var firstDto = new RegisterDto
        {
            FirstName = "First",
            LastName = "User",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            firstDto);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondDto = new RegisterDto
        {
            FirstName = "Second",
            LastName = "User",
            Email = email,
            PhoneNumber = "+91-9876543211",
            Country = "India",
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            secondDto);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }


    // =========================================================
    // LOGIN
    // =========================================================

    [Fact]
    public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
    {
        // Arrange
        await SeedRoleAsync();

        var email =
            $"login-{Guid.NewGuid():N}@example.com";

        var registerDto = new RegisterDto
        {
            FirstName = "Login",
            LastName = "User",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "password123"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }


    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email =
                $"nonexistent-{Guid.NewGuid():N}@example.com",
            Password = "wrong-password"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    // =========================================================
    // PROFILE
    // =========================================================

    [Fact]
    public async Task GetProfile_ShouldReturnOk_WhenUserExists()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act
        var response = await _client.GetAsync(
            "/api/auth/profile");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var profile =
            await response.Content
                .ReadFromJsonAsync<UserProfileDto>();

        Assert.NotNull(profile);
        Assert.Equal("Profile", profile.FirstName);
        Assert.Equal("User", profile.LastName);
    }


    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(
            "/api/auth/profile");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var token = await RegisterAndLoginAsync();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        // Decode the token's user ID indirectly by finding
        // the current user from the database using the email
        // used during registration is intentionally avoided here.
        //
        // Instead, remove the user represented by the token.
        //
        // We get the authenticated user's email from the JWT
        // in the test by registering a known email.

        var email =
            $"deleted-{Guid.NewGuid():N}@example.com";

        var registerDto = new RegisterDto
        {
            FirstName = "Deleted",
            LastName = "User",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "password123"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(loginResult);

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.Token);

        // Remove the user from the database.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var user = db.Users
                .FirstOrDefault(u => u.Email == email);

            Assert.NotNull(user);

            db.Users.Remove(user!);

            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync(
            "/api/auth/profile");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}