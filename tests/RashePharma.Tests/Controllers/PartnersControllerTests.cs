using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.DTOs.Partners;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Tests.Infrastructure;

namespace RashePharma.Tests.Controllers;

public class PartnersControllerTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PartnersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ============================================================
    // PARTNER REQUESTS
    // ============================================================

    [Fact]
    public async Task GetAllRequests_ShouldReturnOk_WhenRequestsExist()
    {
        await CreatePartnerRequestAsync();

        var response =
            await _client.GetAsync("/api/Partners/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var requests =
            await response.Content
                .ReadFromJsonAsync<List<PartnerRequestListDto>>();

        Assert.NotNull(requests);
        Assert.NotEmpty(requests);
    }

    [Fact]
    public async Task GetAllRequests_ShouldReturnEmptyList_WhenNoRequestsExist()
    {
        var response =
            await _client.GetAsync("/api/Partners/requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var requests =
            await response.Content
                .ReadFromJsonAsync<List<PartnerRequestListDto>>();

        Assert.NotNull(requests);
    }

    [Fact]
    public async Task GetRequestById_ShouldReturnOk_WhenRequestExists()
    {
        var request =
            await CreatePartnerRequestAsync();

        var response =
            await _client.GetAsync(
                $"/api/Partners/requests/{request.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PartnerRequestDetailsDto>();

        Assert.NotNull(result);

        Assert.Equal(
            request.Id,
            result.Id);

        Assert.Equal(
            request.CompanyName,
            result.CompanyName);

        Assert.Equal(
            request.ContactPerson,
            result.ContactPerson);

        Assert.Equal(
            request.Email,
            result.Email);
    }

    [Fact]
    public async Task GetRequestById_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var response =
            await _client.GetAsync(
                "/api/Partners/requests/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMyRequests_ShouldReturnOk_WhenUserHasRequests()
    {
        var userId = 3001;

        await CreateUserAsync(userId);

        await CreatePartnerRequestAsync(userId);

        var response =
            await _client.GetAsync(
                $"/api/Partners/requests/user/{userId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var requests =
            await response.Content
                .ReadFromJsonAsync<List<PartnerRequestListDto>>();

        Assert.NotNull(requests);
        Assert.NotEmpty(requests);
    }

    [Fact]
    public async Task GetMyRequests_ShouldReturnEmptyList_WhenUserHasNoRequests()
    {
        var userId = 3002;

        await CreateUserAsync(userId);

        var response =
            await _client.GetAsync(
                $"/api/Partners/requests/user/{userId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var requests =
            await response.Content
                .ReadFromJsonAsync<List<PartnerRequestListDto>>();

        Assert.NotNull(requests);
        Assert.Empty(requests);
    }

    [Fact]
    public async Task CreateRequest_ShouldReturnOk_WhenValidRequestIsProvided()
    {
        var dto = new PartnerRequestCreateDto
        {
            CompanyName = "ABC Pharma Pvt Ltd",
            ContactPerson = "Rahul Sharma",
            Email = "rahul@abcpharma.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "10000 units/month",
            Message = "Interested in becoming a distribution partner."
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/Partners/requests",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PartnerRequestDetailsDto>();

        Assert.NotNull(result);

        Assert.True(result.Id > 0);

        Assert.Equal(
            "ABC Pharma Pvt Ltd",
            result.CompanyName);

        Assert.Equal(
            "Rahul Sharma",
            result.ContactPerson);

        Assert.Equal(
            "rahul@abcpharma.com",
            result.Email);

        Assert.Equal(
            "India",
            result.Country);

        Assert.Equal(
            "Distributor",
            result.BusinessType);

        Assert.Equal(
            "10000 units/month",
            result.ExpectedVolume);

        Assert.Equal(
            "Interested in becoming a distribution partner.",
            result.Message);

        Assert.Equal(
            "Pending",
            result.Status);
    }

    [Fact]
    public async Task CreateRequest_ShouldReturnOk_WhenOptionalFieldsAreNull()
    {
        var dto = new PartnerRequestCreateDto
        {
            CompanyName = "Simple Pharma",
            ContactPerson = "Test Person",
            Email = "simple@example.com",
            Country = "India"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/Partners/requests",
                dto);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PartnerRequestDetailsDto>();

        Assert.NotNull(result);

        Assert.True(result.Id > 0);

        Assert.Equal(
            "Simple Pharma",
            result.CompanyName);

        Assert.Equal(
            "Test Person",
            result.ContactPerson);

        Assert.Equal(
            "simple@example.com",
            result.Email);

        Assert.Null(result.PhoneNumber);
        Assert.Null(result.BusinessType);
        Assert.Null(result.ExpectedVolume);
        Assert.Null(result.Message);

        Assert.Equal(
            "Pending",
            result.Status);
    }

    [Fact]
    public async Task UpdateRequestStatus_ShouldReturnNoContent_WhenRequestExists()
    {
        var request =
            await CreatePartnerRequestAsync();

        var dto = new UpdatePartnerRequestStatusDto
        {
            Status = "Approved"
        };

        var response =
            await _client.PatchAsJsonAsync(
                $"/api/Partners/requests/{request.Id}/status",
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
            await db.PartnerRequests
                .FirstAsync(r => r.Id == request.Id);

        Assert.Equal(
            "Approved",
            updated.Status);
    }

    [Fact]
    public async Task UpdateRequestStatus_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var dto = new UpdatePartnerRequestStatusDto
        {
            Status = "Approved"
        };

        var response =
            await _client.PatchAsJsonAsync(
                "/api/Partners/requests/999999/status",
                dto);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // ============================================================
    // PARTNERS
    // ============================================================

    [Fact]
    public async Task GetAllPartners_ShouldReturnOk_WhenPartnersExist()
    {
        await CreatePartnerAsync();

        var response =
            await _client.GetAsync("/api/Partners");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var partners =
            await response.Content
                .ReadFromJsonAsync<List<PartnerDto>>();

        Assert.NotNull(partners);
        Assert.NotEmpty(partners);
    }

    [Fact]
    public async Task GetAllPartners_ShouldReturnEmptyList_WhenNoPartnersExist()
    {
        var response =
            await _client.GetAsync("/api/Partners");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var partners =
            await response.Content
                .ReadFromJsonAsync<List<PartnerDto>>();

        Assert.NotNull(partners);
    }

    [Fact]
    public async Task GetPartnerById_ShouldReturnOk_WhenPartnerExists()
    {
        var partner =
            await CreatePartnerAsync();

        var response =
            await _client.GetAsync(
                $"/api/Partners/{partner.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PartnerDto>();

        Assert.NotNull(result);

        Assert.Equal(
            partner.Id,
            result.Id);

        Assert.Equal(
            partner.CompanyName,
            result.CompanyName);

        Assert.Equal(
            partner.ContactPerson,
            result.ContactPerson);

        Assert.Equal(
            partner.Email,
            result.Email);
    }

    [Fact]
    public async Task GetPartnerById_ShouldReturnNotFound_WhenPartnerDoesNotExist()
    {
        var response =
            await _client.GetAsync(
                "/api/Partners/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetMyPartner_ShouldReturnOk_WhenUserHasPartner()
    {
        var userId = 3003;

        await CreateUserAsync(userId);

        await CreatePartnerAsync(userId);

        var response =
            await _client.GetAsync(
                $"/api/Partners/user/{userId}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PartnerDto>();

        Assert.NotNull(result);

        Assert.Equal(
            "Test Partner Company",
            result.CompanyName);

        Assert.Equal(
            "Partner Contact",
            result.ContactPerson);
    }

    [Fact]
    public async Task GetMyPartner_ShouldReturnNotFound_WhenUserHasNoPartner()
    {
        var userId = 3004;

        await CreateUserAsync(userId);

        var response =
            await _client.GetAsync(
                $"/api/Partners/user/{userId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private async Task<PartnerRequest> CreatePartnerRequestAsync(
        int? userId = null)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var request = new PartnerRequest
        {
            UserId = userId,
            CompanyName =
                $"Test Partner Company {Guid.NewGuid():N}",
            ContactPerson = "Test Contact",
            Email =
                $"partner-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "5000 units/month",
            Message = "Test partner request.",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        db.PartnerRequests.Add(request);

        await db.SaveChangesAsync();

        return request;
    }

    private async Task<Partner> CreatePartnerAsync(
        int? userId = null)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var partner = new Partner
        {
            UserId = userId,
            CompanyName = "Test Partner Company",
            ContactPerson = "Partner Contact",
            Email =
                $"partner-{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            RegistrationNumber = "REG-TEST-001",
            TaxIdentificationNumber = "TAX-TEST-001",
            Status = "Active",
            JoinedAt = DateTime.UtcNow
        };

        db.Partners.Add(partner);

        await db.SaveChangesAsync();

        return partner;
    }

    private async Task<User> CreateUserAsync(int userId)
    {
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var db =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var existingUser =
            await db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

        if (existingUser != null)
            return existingUser;

        var role =
            await db.Roles
                .FirstOrDefaultAsync(r => r.Id == 2);

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
                $"partner-user-{userId}@example.com",
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
}