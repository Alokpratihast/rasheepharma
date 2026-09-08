using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Addresses;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;
using Xunit;

namespace RashePharma.Tests.Controllers;

public class AddressesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AddressesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // =========================================================
    // AUTHENTICATION HELPERS
    // =========================================================

    private async Task<(int UserId, string Token)> CreateAuthenticatedUserAsync()
    {
        await SeedRoleAsync();

        var email = $"address-{Guid.NewGuid():N}@example.com";
        var password = "Test@12345";

        var registerDto = new RegisterDto
        {
            FirstName = "Address",
            LastName = "TestUser",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = password
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            registerDto);

        registerResponse.EnsureSuccessStatusCode();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            loginDto);

        loginResponse.EnsureSuccessStatusCode();

        var authResponse =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(authResponse);

        SetBearerToken(authResponse.Token);

        return (authResponse.UserId, authResponse.Token);
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private void ClearBearerToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task SeedRoleAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var role = await db.Roles.FindAsync(2);

        if (role == null)
        {
            db.Roles.Add(new Role
            {
                Id = 2,
                Name = "Customer"
            });

            await db.SaveChangesAsync();
        }
    }

    // =========================================================
    // DATABASE HELPERS
    // =========================================================

    private async Task<int> CreateAddressAsync(int userId)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var address = new Address
        {
            UserId = userId,
            AddressLine1 = "123 Test Street",
            AddressLine2 = "Test Area",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Shipping",
            IsDefault = false
        };

        db.Addresses.Add(address);

        await db.SaveChangesAsync();

        return address.Id;
    }

    // =========================================================
    // GET MY ADDRESSES
    // =========================================================

    [Fact]
    public async Task GetMyAddresses_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        ClearBearerToken();

        // Act
        var response = await _client.GetAsync(
            "/api/addresses");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMyAddresses_ShouldReturnOk_WhenUserHasAddresses()
    {
        // Arrange
        var (userId, _) = await CreateAuthenticatedUserAsync();

        await CreateAddressAsync(userId);

        // Act
        var response = await _client.GetAsync(
            "/api/addresses");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var addresses =
            await response.Content
                .ReadFromJsonAsync<List<AddressListDto>>();

        Assert.NotNull(addresses);
        Assert.NotEmpty(addresses);
    }

    [Fact]
    
public async Task GetMyAddresses_ShouldReturnOnlyCurrentUsersAddresses()
{
    // Arrange
    var (user1Id, _) = await CreateAuthenticatedUserAsync();

    var user1AddressId = await CreateAddressAsync(user1Id);

    var (user2Id, user2Token) =
        await CreateAuthenticatedUserAsync();

    await CreateAddressAsync(user2Id);

    SetBearerToken(user2Token);

    // Act
    var response = await _client.GetAsync(
        "/api/addresses");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var addresses =
        await response.Content
            .ReadFromJsonAsync<List<AddressListDto>>();

    Assert.NotNull(addresses);
    Assert.NotEmpty(addresses);

    // User 2's address list must not contain
    // User 1's address.
    Assert.DoesNotContain(
        addresses,
        address => address.Id == user1AddressId);
}
   
        


    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAddressBelongsToCurrentUser()
    {
        // Arrange
        var (userId, _) = await CreateAuthenticatedUserAsync();

        var addressId = await CreateAddressAsync(userId);

        // Act
        var response = await _client.GetAsync(
            $"/api/addresses/{addressId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var address =
            await response.Content
                .ReadFromJsonAsync<AddressDetailsDto>();

        Assert.NotNull(address);
        Assert.Equal(addressId, address.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var (ownerUserId, _) =
            await CreateAuthenticatedUserAsync();

        var addressId =
            await CreateAddressAsync(ownerUserId);

        var (_, anotherUserToken) =
            await CreateAuthenticatedUserAsync();

        SetBearerToken(anotherUserToken);

        // Act
        var response = await _client.GetAsync(
            $"/api/addresses/{addressId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        ClearBearerToken();

        var dto = new AddressCreateDto
        {
            AddressLine1 = "456 New Street",
            AddressLine2 = "Business Area",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560002",
            Country = "India",
            AddressType = "Billing",
            IsDefault = false
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/addresses",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var (_, _) = await CreateAuthenticatedUserAsync();

        var dto = new AddressCreateDto
        {
            AddressLine1 = "456 New Street",
            AddressLine2 = "Business Area",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560002",
            Country = "India",
            AddressType = "Billing",
            IsDefault = false
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/addresses",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var address =
            await response.Content
                .ReadFromJsonAsync<AddressDetailsDto>();

        Assert.NotNull(address);
        Assert.True(address.Id > 0);
        Assert.Equal(
            dto.AddressLine1,
            address.AddressLine1);
        Assert.Equal(
            dto.City,
            address.City);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Fact]
    public async Task Update_ShouldReturnOk_WhenAddressBelongsToCurrentUser()
    {
        // Arrange
        var (userId, _) = await CreateAuthenticatedUserAsync();

        var addressId =
            await CreateAddressAsync(userId);

        var dto = new AddressUpdateDto
        {
            AddressLine1 = "Updated Street",
            AddressLine2 = "Updated Area",
            City = "Mysuru",
            State = "Karnataka",
            PostalCode = "570001",
            Country = "India",
            AddressType = "Shipping",
            IsDefault = true
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/addresses/{addressId}",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var address =
            await response.Content
                .ReadFromJsonAsync<AddressDetailsDto>();

        Assert.NotNull(address);
        Assert.Equal(addressId, address.Id);
        Assert.Equal(
            dto.AddressLine1,
            address.AddressLine1);
        Assert.Equal(
            dto.City,
            address.City);
        Assert.True(address.IsDefault);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var (ownerUserId, _) =
            await CreateAuthenticatedUserAsync();

        var addressId =
            await CreateAddressAsync(ownerUserId);

        var (_, anotherUserToken) =
            await CreateAuthenticatedUserAsync();

        SetBearerToken(anotherUserToken);

        var dto = new AddressUpdateDto
        {
            AddressLine1 = "Unauthorized Update",
            AddressLine2 = "Test Area",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560003",
            Country = "India",
            AddressType = "Shipping",
            IsDefault = false
        };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/addresses/{addressId}",
            dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenAddressBelongsToCurrentUser()
    {
        // Arrange
        var (userId, _) =
            await CreateAuthenticatedUserAsync();

        var addressId =
            await CreateAddressAsync(userId);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/addresses/{addressId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var (ownerUserId, _) =
            await CreateAuthenticatedUserAsync();

        var addressId =
            await CreateAddressAsync(ownerUserId);

        var (_, anotherUserToken) =
            await CreateAuthenticatedUserAsync();

        SetBearerToken(anotherUserToken);

        // Act
        var response = await _client.DeleteAsync(
            $"/api/addresses/{addressId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}