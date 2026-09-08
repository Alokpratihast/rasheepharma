using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.DTOs.Enquiries;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;
using Xunit;

namespace RashePharma.Tests.Controllers;

public class EnquiriesControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EnquiriesControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // =========================================================
    // AUTHENTICATED CUSTOMER
    // =========================================================

    private async Task<(int UserId, string Token)>
        CreateAuthenticatedUserAsync()
    {
        await SeedCustomerRoleAsync();

        var email =
            $"enquiry-auth-{Guid.NewGuid():N}@example.com";

        var registerDto = new RegisterDto
        {
            FirstName = "Enquiry",
            LastName = "TestUser",
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = "India",
            Password = "password123"
        };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerDto);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var registerResult =
            await registerResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(registerResult);
        Assert.True(registerResult.UserId > 0);

        // Login through the real API.
        var loginDto = new LoginDto
        {
            Email = email,
            Password = "password123"
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(loginResult);
        Assert.False(
            string.IsNullOrWhiteSpace(loginResult.Token));

        return (
            loginResult.UserId,
            loginResult.Token
        );
    }

    private void SetBearerToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }

    private void ClearBearerToken()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    // =========================================================
    // ADMIN AUTHENTICATION
    // =========================================================

    private async Task<string> CreateAdminTokenAsync()
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var adminRole =
            await db.Roles.FirstOrDefaultAsync(r => r.Id == 1);

        if (adminRole == null)
        {
            adminRole = new Role
            {
                Id = 1,
                Name = "Admin"
            };

            db.Roles.Add(adminRole);

            await db.SaveChangesAsync();
        }

        var email =
            $"admin-enquiry-{Guid.NewGuid():N}@example.com";

        var admin = new User
        {
            FirstName = "Test",
            LastName = "Admin",
            Email = email,
            PhoneNumber = "+91-9999999999",
            Country = "India",
            RoleId = adminRole.Id,
            IsActive = true
        };

        var passwordHasher =
            new PasswordHasher<User>();

        admin.PasswordHash =
            passwordHasher.HashPassword(
                admin,
                "adminPassword123");

        db.Users.Add(admin);

        await db.SaveChangesAsync();

        var loginDto = new LoginDto
        {
            Email = email,
            Password = "adminPassword123"
        };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginDto);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var result =
            await loginResponse.Content
                .ReadFromJsonAsync<AuthResponseDto>();

        Assert.NotNull(result);
        Assert.False(
            string.IsNullOrWhiteSpace(result.Token));

        return result.Token;
    }

    // =========================================================
    // ROLE
    // =========================================================

    private async Task SeedCustomerRoleAsync()
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        if (!await db.Roles.AnyAsync(r => r.Id == 2))
        {
            db.Roles.Add(
                new Role
                {
                    Id = 2,
                    Name = "Customer"
                });

            await db.SaveChangesAsync();
        }
    }

    // =========================================================
    // GET ALL
    // =========================================================

    [Fact]
    public async Task GetAll_ShouldReturnOk_ForCustomer()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        await CreateEnquiryAsync(
            "Test Customer",
            "customer1@example.com",
            "India",
            setup.UserId);

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                "/api/Enquiries");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var enquiries =
            await response.Content
                .ReadFromJsonAsync<List<EnquiryListDto>>();

        Assert.NotNull(enquiries);
        Assert.NotEmpty(enquiries);
    }

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        ClearBearerToken();

        var response =
            await _client.GetAsync(
                "/api/Enquiries");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllEnquiries_ForAdmin()
    {
        var customer =
            await CreateAuthenticatedUserAsync();

        await CreateEnquiryAsync(
            "Customer One",
            "customer-one@example.com",
            "India",
            customer.UserId);

        var adminToken =
            await CreateAdminTokenAsync();

        SetBearerToken(adminToken);

        var response =
            await _client.GetAsync(
                "/api/Enquiries");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var enquiries =
            await response.Content
                .ReadFromJsonAsync<List<EnquiryListDto>>();

        Assert.NotNull(enquiries);
        Assert.NotEmpty(enquiries);
    }

    // =========================================================
    // GET BY USER ID
    // =========================================================

    // NOTE:
    // Current API does not expose:
    // GET /api/Enquiries/user/{userId}
    //
    // GET /api/Enquiries is now role-aware:
    // Customer -> own enquiries
    // Admin    -> all enquiries

    [Fact]
    public async Task GetByUserId_ShouldReturnOk_ForAdmin()
    {
        var customer =
            await CreateAuthenticatedUserAsync();

        await CreateEnquiryAsync(
            "User Customer",
            "user@example.com",
            "India",
            customer.UserId);

        var adminToken =
            await CreateAdminTokenAsync();

        SetBearerToken(adminToken);

        var response =
            await _client.GetAsync(
                "/api/Enquiries");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var enquiries =
            await response.Content
                .ReadFromJsonAsync<List<EnquiryListDto>>();

        Assert.NotNull(enquiries);
        Assert.NotEmpty(enquiries);
    }

    [Fact]
    public async Task GetByUserId_ShouldReturnOk_ForCustomer()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        await CreateEnquiryAsync(
            "Customer Own Enquiry",
            "customer-own@example.com",
            "India",
            setup.UserId);

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                "/api/Enquiries");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var enquiries =
            await response.Content
                .ReadFromJsonAsync<List<EnquiryListDto>>();

        Assert.NotNull(enquiries);
        Assert.NotEmpty(enquiries);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Fact]
    public async Task GetById_ShouldReturnOk_ForCustomerOwnEnquiry()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "ID Customer",
                "idcustomer@example.com",
                "India",
                setup.UserId);

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                $"/api/Enquiries/{enquiry.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<EnquiryDetailsDto>();

        Assert.NotNull(result);

        Assert.Equal(
            enquiry.Id,
            result.Id);

        Assert.Equal(
            "ID Customer",
            result.CustomerName);

        Assert.Equal(
            "idcustomer@example.com",
            result.Email);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEnquiryDoesNotBelongToUser()
    {
        var owner =
            await CreateAuthenticatedUserAsync();

        var anotherUser =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Owner Customer",
                "owner@example.com",
                "India",
                owner.UserId);

        SetBearerToken(anotherUser.Token);

        var response =
            await _client.GetAsync(
                $"/api/Enquiries/{enquiry.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenEnquiryDoesNotExist()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                "/api/Enquiries/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // GET BY ENQUIRY NUMBER
    // =========================================================

    [Fact]
    public async Task GetByEnquiryNumber_ShouldReturnOk_ForCustomerOwnEnquiry()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Number Customer",
                "numbercustomer@example.com",
                "India",
                setup.UserId);

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                $"/api/Enquiries/number/{enquiry.EnquiryNumber}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<EnquiryDetailsDto>();

        Assert.NotNull(result);

        Assert.Equal(
            enquiry.Id,
            result.Id);

        Assert.Equal(
            enquiry.EnquiryNumber,
            result.EnquiryNumber);
    }

    [Fact]
    public async Task GetByEnquiryNumber_ShouldReturnNotFound_WhenEnquiryDoesNotBelongToUser()
    {
        var owner =
            await CreateAuthenticatedUserAsync();

        var anotherUser =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Owner Customer",
                "owner-number@example.com",
                "India",
                owner.UserId);

        SetBearerToken(anotherUser.Token);

        var response =
            await _client.GetAsync(
                $"/api/Enquiries/number/{enquiry.EnquiryNumber}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByEnquiryNumber_ShouldReturnOk_ForAdmin()
    {
        var customer =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Admin Customer",
                "admin-view@example.com",
                "India",
                customer.UserId);

        var adminToken =
            await CreateAdminTokenAsync();

        SetBearerToken(adminToken);

        var response =
            await _client.GetAsync(
                $"/api/Enquiries/number/{enquiry.EnquiryNumber}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByEnquiryNumber_ShouldReturnNotFound_WhenEnquiryDoesNotExist()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        SetBearerToken(setup.Token);

        var response =
            await _client.GetAsync(
                "/api/Enquiries/number/ENQ-NOT-FOUND-999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Fact]
    public async Task Create_ShouldReturnOk_WhenValidEnquiryIsProvided()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        var variantId =
            await CreateProductVariantAsync();

        var dto = new CreateEnquiryDto
        {
            CustomerName = "New Customer",
            Email = "newcustomer@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            Message =
                "Interested in pharmaceutical products.",

            Items = new List<CreateEnquiryItemDto>
            {
                new()
                {
                    ProductVariantId = variantId,
                    Quantity = 100,
                    Message =
                        "Please provide quotation."
                }
            }
        };

        SetBearerToken(setup.Token);

        var response =
            await _client.PostAsJsonAsync(
                "/api/Enquiries",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<EnquiryDetailsDto>();

        Assert.NotNull(result);

        Assert.True(result.Id > 0);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.EnquiryNumber));

        Assert.Equal(
            "New Customer",
            result.CustomerName);

        Assert.Equal(
            "newcustomer@example.com",
            result.Email);

        Assert.Equal(
            "India",
            result.Country);

        Assert.Equal(
            "Distributor",
            result.BusinessType);

        Assert.Equal(
            "Pending",
            result.Status);

        Assert.Single(result.Items);

        Assert.Equal(
            variantId,
            result.Items[0].ProductVariantId);

        Assert.Equal(
            100,
            result.Items[0].Quantity);

        Assert.Equal(
            "Please provide quotation.",
            result.Items[0].Message);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenEnquiryHasNoItems()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        var dto = new CreateEnquiryDto
        {
            CustomerName = "No Item Customer",
            Email = "noitem@example.com",
            PhoneNumber = "+91-9999999999",
            Country = "India",
            BusinessType = "Pharmacy",
            Message = "General enquiry.",
            Items = new List<CreateEnquiryItemDto>()
        };

        SetBearerToken(setup.Token);

        var response =
            await _client.PostAsJsonAsync(
                "/api/Enquiries",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<EnquiryDetailsDto>();

        Assert.NotNull(result);

        Assert.True(result.Id > 0);

        Assert.Equal(
            "No Item Customer",
            result.CustomerName);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        ClearBearerToken();

        var dto = new CreateEnquiryDto
        {
            CustomerName = "Guest Customer",
            Email = "guest@example.com",
            PhoneNumber = "+91-9999999999",
            Country = "India",
            BusinessType = "Pharmacy",
            Message = "Guest enquiry.",
            Items = new List<CreateEnquiryItemDto>()
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/Enquiries",
                dto);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // =========================================================
    // UPDATE STATUS
    // =========================================================

    [Fact]
    public async Task UpdateStatus_ShouldReturnNoContent_ForAdmin()
    {
        var customer =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Status Customer",
                "statuscustomer@example.com",
                "India",
                customer.UserId);

        var adminToken =
            await CreateAdminTokenAsync();

        SetBearerToken(adminToken);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted",
            Comment =
                "Customer has been contacted."
        };

        var response =
            await _client.PatchAsJsonAsync(
                $"/api/Enquiries/{enquiry.Id}/status",
                dto);

        Assert.Equal(
            HttpStatusCode.NoContent,
            response.StatusCode);

        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var updated =
            await db.Enquiries
                .FirstAsync(e =>
                    e.Id == enquiry.Id);

        Assert.Equal(
            "Contacted",
            updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnForbidden_ForCustomer()
    {
        var setup =
            await CreateAuthenticatedUserAsync();

        var enquiry =
            await CreateEnquiryAsync(
                "Status Customer",
                "statuscustomer2@example.com",
                "India",
                setup.UserId);

        SetBearerToken(setup.Token);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted",
            Comment =
                "Customer should not update status."
        };

        var response =
            await _client.PatchAsJsonAsync(
                $"/api/Enquiries/{enquiry.Id}/status",
                dto);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenEnquiryDoesNotExist()
    {
        var adminToken =
            await CreateAdminTokenAsync();

        SetBearerToken(adminToken);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted",
            Comment = "Test comment."
        };

        var response =
            await _client.PatchAsJsonAsync(
                "/api/Enquiries/999999/status",
                dto);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        ClearBearerToken();

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted",
            Comment = "Test comment."
        };

        var response =
            await _client.PatchAsJsonAsync(
                "/api/Enquiries/999999/status",
                dto);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // =========================================================
    // DATABASE HELPERS
    // =========================================================

    private async Task<Enquiry> CreateEnquiryAsync(
        string customerName,
        string email,
        string country,
        int? userId = null)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var enquiry = new Enquiry
        {
            CustomerName = customerName,
            Email = email,
            PhoneNumber = "+91-9876543210",
            Country = country,
            BusinessType = "Distributor",
            Message = "Test enquiry",
            UserId = userId,
            EnquiryNumber =
                $"ENQ-TEST-{Guid.NewGuid():N}",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.Enquiries.Add(enquiry);

        await db.SaveChangesAsync();

        return enquiry;
    }

    private async Task<int> CreateProductVariantAsync()
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var category = new Category
        {
            Name =
                $"Enquiry Category {Guid.NewGuid():N}",

            Slug =
                $"enquiry-category-{Guid.NewGuid():N}",

            Description =
                "Category for enquiry controller tests",

            IsActive = true
        };

        db.Categories.Add(category);

        await db.SaveChangesAsync();

        var product = new Product
        {
            Name =
                $"Enquiry Product {Guid.NewGuid():N}",

            Slug =
                $"enquiry-product-{Guid.NewGuid():N}",

            GenericName = "Test Generic",

            Composition = "Test Composition",

            DosageForm = "Tablet",

            Description =
                "Product for enquiry controller tests",

            Manufacturer = "Rashe Pharma",

            CategoryId = category.Id,

            IsActive = true
        };

        db.Products.Add(product);

        await db.SaveChangesAsync();

        var variant = new ProductVariant
        {
            ProductId = product.Id,

            Strength = "500 mg",

            PackSize = "10 Tablets",

            Price = 100m,

            SKU =
                $"ENQ-SKU-{Guid.NewGuid():N}",

            StockQuantity = 1000,

            IsActive = true
        };

        db.ProductVariants.Add(variant);

        await db.SaveChangesAsync();

        return variant.Id;
    }
}