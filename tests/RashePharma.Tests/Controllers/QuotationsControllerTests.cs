using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Quotations;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;

namespace RashePharma.Tests.Controllers;

public class QuotationsControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuotationsControllerTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // =========================================================
    // Authentication helpers
    // =========================================================

    private HttpClient CreateAuthenticatedClient(
        int userId,
        string role = "Customer")
    {
        var client = _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        client.DefaultRequestHeaders.Add(
            "X-Test-Role",
            role);

        return client;
    }

    // =========================================================
    // GetAll
    // =========================================================

    [Fact]
    public async Task GetAll_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        var response =
            await _client.GetAsync("/api/Quotations");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WhenCustomerIsAuthenticated()
    {
        var userId = 2001;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        await CreateQuotationAsync(
            enquiry.Id,
            userId);

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.GetAsync(
                "/api/Quotations");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.NotEmpty(quotations);

        Assert.All(
            quotations,
            q => Assert.Equal(
                enquiry.Id,
                q.EnquiryId));
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllQuotations_WhenAdminIsAuthenticated()
    {
        var user1 = 2101;
        var user2 = 2102;

        await CreateUserAsync(user1);
        await CreateUserAsync(user2);

        var enquiry1 =
            await CreateEnquiryAsync(user1);

        var enquiry2 =
            await CreateEnquiryAsync(user2);

        await CreateQuotationAsync(
            enquiry1.Id,
            user1);

        await CreateQuotationAsync(
            enquiry2.Id,
            user2);

        var client =
            CreateAuthenticatedClient(
                9001,
                "Admin");

        var response =
            await client.GetAsync(
                "/api/Quotations");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);

        Assert.Contains(
            quotations,
            q => q.EnquiryId == enquiry1.Id);

        Assert.Contains(
            quotations,
            q => q.EnquiryId == enquiry2.Id);
    }

    // =========================================================
    // GetByUserId
    // =========================================================

    [Fact]
    public async Task GetByUserId_ShouldReturnForbidden_WhenCustomerCallsIt()
    {
        var userId = 2201;

        await CreateUserAsync(userId);

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/user/{userId}");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByUserId_ShouldReturnOk_WhenAdminCallsIt()
    {
        var userId = 2202;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        await CreateQuotationAsync(
            enquiry.Id,
            userId);

        var client =
            CreateAuthenticatedClient(
                9002,
                "Admin");

        var response =
            await client.GetAsync(
                $"/api/Quotations/user/{userId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.NotEmpty(quotations);

        Assert.All(
            quotations,
            q => Assert.Equal(
                enquiry.Id,
                q.EnquiryId));
    }

    [Fact]
    public async Task GetByUserId_ShouldReturnEmptyList_WhenAdminUserHasNoQuotations()
    {
        var userId = 2203;

        await CreateUserAsync(userId);

        var client =
            CreateAuthenticatedClient(
                9003,
                "Admin");

        var response =
            await client.GetAsync(
                $"/api/Quotations/user/{userId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.Empty(quotations);
    }

    // =========================================================
    // GetByEnquiryId
    // =========================================================

    [Fact]
    public async Task GetByEnquiryId_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        var response =
            await _client.GetAsync(
                "/api/Quotations/enquiry/999999");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByEnquiryId_ShouldReturnOk_WhenCustomerOwnsQuotation()
    {
        var userId = 2301;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        await CreateQuotationAsync(
            enquiry.Id,
            userId);

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/enquiry/{enquiry.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.NotEmpty(quotations);

        Assert.Contains(
            quotations,
            q => q.EnquiryId == enquiry.Id);
    }

    [Fact]
    public async Task GetByEnquiryId_ShouldReturnEmpty_WhenCustomerDoesNotOwnQuotation()
    {
        var ownerId = 2302;
        var otherUserId = 2303;

        await CreateUserAsync(ownerId);
        await CreateUserAsync(otherUserId);

        var enquiry =
            await CreateEnquiryAsync(ownerId);

        await CreateQuotationAsync(
            enquiry.Id,
            ownerId);

        var client =
            CreateAuthenticatedClient(otherUserId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/enquiry/{enquiry.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.Empty(quotations);
    }

    [Fact]
    public async Task GetByEnquiryId_ShouldReturnOk_WhenAdminCallsIt()
    {
        var userId = 2304;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        await CreateQuotationAsync(
            enquiry.Id,
            userId);

        var client =
            CreateAuthenticatedClient(
                9004,
                "Admin");

        var response =
            await client.GetAsync(
                $"/api/Quotations/enquiry/{enquiry.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var quotations =
            await response.Content
                .ReadFromJsonAsync<List<QuotationListDto>>();

        Assert.NotNull(quotations);
        Assert.NotEmpty(quotations);
    }

    // =========================================================
    // GetById
    // =========================================================

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCustomerOwnsQuotation()
    {
        var userId = 2401;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                userId);

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/{quotation.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<QuotationDetailsDto>();

        Assert.NotNull(result);

        Assert.Equal(
            quotation.Id,
            result.Id);

        Assert.Equal(
            enquiry.Id,
            result.EnquiryId);

        Assert.Equal(
            "USD",
            result.Currency);

        Assert.Equal(
            "Draft",
            result.Status);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenQuotationBelongsToAnotherUser()
    {
        var ownerId = 2402;
        var otherUserId = 2403;

        await CreateUserAsync(ownerId);
        await CreateUserAsync(otherUserId);

        var enquiry =
            await CreateEnquiryAsync(ownerId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                ownerId);

        var client =
            CreateAuthenticatedClient(otherUserId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/{quotation.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenQuotationDoesNotExist()
    {
        var client =
            CreateAuthenticatedClient(2404);

        var response =
            await client.GetAsync(
                "/api/Quotations/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenAdminRequestsAnotherUsersQuotation()
    {
        var ownerId = 2405;

        await CreateUserAsync(ownerId);

        var enquiry =
            await CreateEnquiryAsync(ownerId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                ownerId);

        var client =
            CreateAuthenticatedClient(
                9005,
                "Admin");

        var response =
            await client.GetAsync(
                $"/api/Quotations/{quotation.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // =========================================================
    // GetByQuoteNumber
    // =========================================================

    [Fact]
    public async Task GetByQuoteNumber_ShouldReturnOk_WhenCustomerOwnsQuotation()
    {
        var userId = 2501;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                userId);

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/number/{quotation.QuoteNumber}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<QuotationDetailsDto>();

        Assert.NotNull(result);

        Assert.Equal(
            quotation.Id,
            result.Id);

        Assert.Equal(
            quotation.QuoteNumber,
            result.QuoteNumber);
    }

    [Fact]
    public async Task GetByQuoteNumber_ShouldReturnNotFound_WhenQuotationBelongsToAnotherUser()
    {
        var ownerId = 2502;
        var otherUserId = 2503;

        await CreateUserAsync(ownerId);
        await CreateUserAsync(otherUserId);

        var enquiry =
            await CreateEnquiryAsync(ownerId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                ownerId);

        var client =
            CreateAuthenticatedClient(otherUserId);

        var response =
            await client.GetAsync(
                $"/api/Quotations/number/{quotation.QuoteNumber}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByQuoteNumber_ShouldReturnNotFound_WhenQuotationDoesNotExist()
    {
        var client =
            CreateAuthenticatedClient(2504);

        var response =
            await client.GetAsync(
                "/api/Quotations/number/QUOTE-NOT-FOUND-999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetByQuoteNumber_ShouldReturnOk_WhenAdminRequestsAnotherUsersQuotation()
    {
        var ownerId = 2505;

        await CreateUserAsync(ownerId);

        var enquiry =
            await CreateEnquiryAsync(ownerId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                ownerId);

        var client =
            CreateAuthenticatedClient(
                9006,
                "Admin");

        var response =
            await client.GetAsync(
                $"/api/Quotations/number/{quotation.QuoteNumber}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // =========================================================
    // Create
    // =========================================================

    [Fact]
    public async Task Create_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        var dto = new CreateQuotationDto
        {
            EnquiryId = 999999,
            Currency = "USD",
            ValidUntil = DateTime.UtcNow.AddDays(30),
            Notes = "Unauthenticated quotation.",
            Items = new List<CreateQuotationItemDto>
            {
                new()
                {
                    ProductVariantId = 1,
                    Quantity = 1,
                    
                }
            }
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/Quotations",
                dto);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WhenValidQuotationIsProvided()
    {
        var userId = 2601;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var variantId =
            await CreateProductVariantAsync();

        var dto = new CreateQuotationDto
        {
            EnquiryId = enquiry.Id,
            Currency = "USD",
            ValidUntil = DateTime.UtcNow.AddDays(30),
            Notes = "Quotation for distributor.",
            Items = new List<CreateQuotationItemDto>
            {
                new()
                {
                    ProductVariantId = variantId,
                    Quantity = 100,
                    
                }
            }
        };

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.PostAsJsonAsync(
                "/api/Quotations",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<QuotationDetailsDto>();

        Assert.NotNull(result);

        Assert.True(
            result.Id > 0);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.QuoteNumber));

        Assert.Equal(
            enquiry.Id,
            result.EnquiryId);

        Assert.Equal(
            "USD",
            result.Currency);

        Assert.Equal(
            "Draft",
            result.Status);

        Assert.Equal(
            "Quotation for distributor.",
            result.Notes);

        Assert.Single(result.Items);

        Assert.Equal(
            variantId,
            result.Items[0].ProductVariantId);

        Assert.Equal(
            100,
            result.Items[0].Quantity);

        Assert.Equal(
            100m,
            result.Items[0].UnitPrice);

        Assert.Equal(
            10000m,
            result.TotalAmount);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenQuotationHasNoItems()
    {
        var userId = 2602;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var dto = new CreateQuotationDto
        {
            EnquiryId = enquiry.Id,
            Currency = "USD",
            ValidUntil = DateTime.UtcNow.AddDays(15),
            Notes = "No item quotation.",
            Items = new List<CreateQuotationItemDto>()
        };

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.PostAsJsonAsync(
                "/api/Quotations",
                dto);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    // =========================================================
    // UpdateStatus
    // =========================================================

    [Fact]
    public async Task UpdateStatus_ShouldReturnForbidden_WhenCustomerCallsIt()
    {
        var userId = 2701;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                userId);

        var dto = new UpdateQuotationStatusDto
        {
            Status = "Sent"
        };

        var client =
            CreateAuthenticatedClient(userId);

        var response =
            await client.PatchAsJsonAsync(
                $"/api/Quotations/{quotation.Id}/status",
                dto);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNoContent_WhenAdminCallsIt()
    {
        var userId = 2702;

        await CreateUserAsync(userId);

        var enquiry =
            await CreateEnquiryAsync(userId);

        var quotation =
            await CreateQuotationAsync(
                enquiry.Id,
                userId);

        var dto = new UpdateQuotationStatusDto
        {
            Status = "Sent"
        };

        var client =
            CreateAuthenticatedClient(
                9007,
                "Admin");

        var response =
            await client.PatchAsJsonAsync(
                $"/api/Quotations/{quotation.Id}/status",
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
            await db.Quotations
                .FirstAsync(
                    q => q.Id == quotation.Id);

        Assert.Equal(
            "Sent",
            updated.Status);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturnNotFound_WhenAdminQuotationDoesNotExist()
    {
        var dto = new UpdateQuotationStatusDto
        {
            Status = "Sent"
        };

        var client =
            CreateAuthenticatedClient(
                9008,
                "Admin");

        var response =
            await client.PatchAsJsonAsync(
                "/api/Quotations/999999/status",
                dto);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // =========================================================
    // Database helpers
    // =========================================================

    private async Task<User> CreateUserAsync(
        int userId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var existingUser =
            await db.Users
                .FirstOrDefaultAsync(
                    u => u.Id == userId);

        if (existingUser != null)
            return existingUser;

        var role =
            await db.Roles
                .FirstOrDefaultAsync(
                    r => r.Id == 2);

        if (role == null)
        {
            role = new Role
            {
                Id = 2,
                Name = "Customer"
            };

            db.Roles.Add(role);

            await db.SaveChangesAsync();
        }

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email =
                $"quotation{userId}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            PasswordHash = "test-password-hash",
            RoleId = role.Id,
            IsActive = true
        };

        db.Users.Add(user);

        await db.SaveChangesAsync();

        return user;
    }

    private async Task<Enquiry> CreateEnquiryAsync(
        int? userId = null)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var enquiry = new Enquiry
        {
            UserId = userId,
            EnquiryNumber =
                $"ENQ-TEST-{Guid.NewGuid():N}",
            CustomerName = "Quotation Customer",
            Email =
                $"quotation-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            Message = "Quotation test enquiry.",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.Enquiries.Add(enquiry);

        await db.SaveChangesAsync();

        return enquiry;
    }

    private async Task<Quotation> CreateQuotationAsync(
        int enquiryId,
        int? userId = null)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var quotation = new Quotation
        {
            EnquiryId = enquiryId,
            UserId = userId,
            QuoteNumber =
                $"QUO-TEST-{Guid.NewGuid():N}",
            TotalAmount = 1000m,
            Currency = "USD",
            Status = "Draft",
            ValidUntil =
                DateTime.UtcNow.AddDays(30),
            Notes = "Test quotation.",
            CreatedAt = DateTime.UtcNow
        };

        db.Quotations.Add(quotation);

        await db.SaveChangesAsync();

        return quotation;
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
                $"Quotation Category {Guid.NewGuid():N}",
            Slug =
                $"quotation-category-{Guid.NewGuid():N}",
            IsActive = true
        };

        db.Categories.Add(category);

        await db.SaveChangesAsync();

        var product = new Product
        {
            Name =
                $"Quotation Product {Guid.NewGuid():N}",
            Slug =
                $"quotation-product-{Guid.NewGuid():N}",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test quotation product.",
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
                $"SKU-QUO-{Guid.NewGuid():N}",
            StockQuantity = 1000,
            IsActive = true
        };

        db.ProductVariants.Add(variant);

        await db.SaveChangesAsync();

        return variant.Id;
    }
}