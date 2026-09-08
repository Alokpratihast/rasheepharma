using Moq;
using RashePharma.Application.DTOs.Partners;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class PartnerServiceTests
{
    private readonly Mock<IPartnerRequestRepository> _requestRepositoryMock;
    private readonly Mock<IPartnerRepository> _partnerRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PartnerService _service;

    public PartnerServiceTests()
    {
        _requestRepositoryMock = new Mock<IPartnerRequestRepository>();
        _partnerRepositoryMock = new Mock<IPartnerRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _service = new PartnerService(
            _requestRepositoryMock.Object,
            _partnerRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    // =========================================================
    // GetAllRequestsAsync
    // =========================================================

    [Fact]
    public async Task GetAllRequestsAsync_ShouldReturnRequests()
    {
        var requests = new List<PartnerRequest>
        {
            CreateRequest(1, "ABC Pharma"),
            CreateRequest(2, "XYZ Healthcare")
        };

        _requestRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(requests);

        var result = await _service.GetAllRequestsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("ABC Pharma", result[0].CompanyName);
        Assert.Equal("XYZ Healthcare", result[1].CompanyName);

        _requestRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllRequestsAsync_ShouldReturnEmptyList_WhenNoRequests()
    {
        _requestRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PartnerRequest>());

        var result = await _service.GetAllRequestsAsync();

        Assert.NotNull(result);
        Assert.Empty(result);

        _requestRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    // =========================================================
    // GetRequestByIdAsync
    // =========================================================

    [Fact]
    public async Task GetRequestByIdAsync_ShouldReturnRequest_WhenFound()
    {
        var request = CreateRequest(1, "ABC Pharma");

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        var result = await _service.GetRequestByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ABC Pharma", result.CompanyName);
        Assert.Equal("John Doe", result.ContactPerson);
        Assert.Equal("john@abc.com", result.Email);
        Assert.Equal("India", result.Country);
        Assert.Equal("Distributor", result.BusinessType);
        Assert.Equal("1000 units/month", result.ExpectedVolume);
        Assert.Equal("Pending", result.Status);

        _requestRepositoryMock.Verify(
            r => r.GetByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetRequestByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((PartnerRequest?)null);

        var result = await _service.GetRequestByIdAsync(999);

        Assert.Null(result);

        _requestRepositoryMock.Verify(
            r => r.GetByIdAsync(999),
            Times.Once);
    }

    // =========================================================
    // GetMyRequestsAsync
    // =========================================================

    [Fact]
    public async Task GetMyRequestsAsync_ShouldReturnUserRequests()
    {
        var userId = 10;

        var requests = new List<PartnerRequest>
        {
            CreateRequest(1, "ABC Pharma"),
            CreateRequest(2, "XYZ Healthcare")
        };

        _requestRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(requests);

        var result = await _service.GetMyRequestsAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("ABC Pharma", result[0].CompanyName);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("XYZ Healthcare", result[1].CompanyName);

        _requestRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task GetMyRequestsAsync_ShouldReturnEmptyList_WhenUserHasNoRequests()
    {
        var userId = 10;

        _requestRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<PartnerRequest>());

        var result = await _service.GetMyRequestsAsync(userId);

        Assert.NotNull(result);
        Assert.Empty(result);

        _requestRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId),
            Times.Once);
    }

    // =========================================================
    // CreateRequestAsync
    // =========================================================

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequest_ForLoggedInUser()
    {
        var userId = 10;

        var dto = CreateRequestDto();

        PartnerRequest? createdRequest = null;

        _requestRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PartnerRequest>()))
            .Callback<PartnerRequest>(request =>
            {
                request.Id = 1;
                createdRequest = request;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateRequestAsync(dto, userId);

        Assert.NotNull(result);

        Assert.Equal(1, result.Id);
        Assert.Equal("ABC Pharma", result.CompanyName);
        Assert.Equal("John Doe", result.ContactPerson);
        Assert.Equal("john@abc.com", result.Email);
        Assert.Equal("+91-9876543210", result.PhoneNumber);
        Assert.Equal("India", result.Country);
        Assert.Equal("Distributor", result.BusinessType);
        Assert.Equal("1000 units/month", result.ExpectedVolume);
        Assert.Equal("Interested in long-term partnership.", result.Message);
        Assert.Equal("Pending", result.Status);

        Assert.NotNull(createdRequest);
        Assert.Equal(userId, createdRequest!.UserId);

        _requestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<PartnerRequest>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldCreateRequest_ForGuestUser()
    {
        var dto = CreateRequestDto();

        PartnerRequest? createdRequest = null;

        _requestRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PartnerRequest>()))
            .Callback<PartnerRequest>(request =>
            {
                request.Id = 2;
                createdRequest = request;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateRequestAsync(dto, null);

        Assert.NotNull(result);

        Assert.Equal(2, result.Id);
        Assert.Equal("ABC Pharma", result.CompanyName);
        Assert.Equal("Pending", result.Status);

        Assert.NotNull(createdRequest);
        Assert.Null(createdRequest!.UserId);

        _requestRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<PartnerRequest>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateRequestAsync_ShouldSetDefaultStatusToPending()
    {
        var dto = CreateRequestDto();

        PartnerRequest? createdRequest = null;

        _requestRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PartnerRequest>()))
            .Callback<PartnerRequest>(request =>
            {
                request.Id = 3;
                createdRequest = request;
            })
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _service.CreateRequestAsync(dto, 10);

        Assert.NotNull(createdRequest);
        Assert.Equal("Pending", createdRequest!.Status);

        Assert.Equal("Pending", result.Status);
    }

    // =========================================================
    // UpdateRequestStatusAsync
    // =========================================================

    [Fact]
    public async Task UpdateRequestStatusAsync_ShouldUpdateStatus_WhenRequestExists()
    {
        var request = CreateRequest(1, "ABC Pharma");

        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(request);

        _requestRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<PartnerRequest>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var dto = new UpdatePartnerRequestStatusDto
        {
            Status = "Approved"
        };

        var result = await _service.UpdateRequestStatusAsync(1, dto);

        Assert.True(result);
        Assert.Equal("Approved", request.Status);

        _requestRepositoryMock.Verify(
            r => r.GetByIdAsync(1),
            Times.Once);

        _requestRepositoryMock.Verify(
            r => r.UpdateAsync(request),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateRequestStatusAsync_ShouldReturnFalse_WhenRequestNotFound()
    {
        _requestRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((PartnerRequest?)null);

        var dto = new UpdatePartnerRequestStatusDto
        {
            Status = "Approved"
        };

        var result = await _service.UpdateRequestStatusAsync(999, dto);

        Assert.False(result);

        _requestRepositoryMock.Verify(
            r => r.GetByIdAsync(999),
            Times.Once);

        _requestRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<PartnerRequest>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // GetAllPartnersAsync
    // =========================================================

    [Fact]
    public async Task GetAllPartnersAsync_ShouldReturnPartners()
    {
        var partners = new List<Partner>
        {
            CreatePartner(1, "ABC Pharma"),
            CreatePartner(2, "XYZ Healthcare")
        };

        _partnerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(partners);

        var result = await _service.GetAllPartnersAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("ABC Pharma", result[0].CompanyName);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("XYZ Healthcare", result[1].CompanyName);

        _partnerRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetAllPartnersAsync_ShouldReturnEmptyList_WhenNoPartners()
    {
        _partnerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Partner>());

        var result = await _service.GetAllPartnersAsync();

        Assert.NotNull(result);
        Assert.Empty(result);

        _partnerRepositoryMock.Verify(
            r => r.GetAllAsync(),
            Times.Once);
    }

    // =========================================================
    // GetPartnerByIdAsync
    // =========================================================

    [Fact]
    public async Task GetPartnerByIdAsync_ShouldReturnPartner_WhenFound()
    {
        var partner = CreatePartner(1, "ABC Pharma");

        _partnerRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(partner);

        var result = await _service.GetPartnerByIdAsync(1);

        Assert.NotNull(result);

        Assert.Equal(1, result.Id);
        Assert.Equal("ABC Pharma", result.CompanyName);
        Assert.Equal("John Doe", result.ContactPerson);
        Assert.Equal("john@abc.com", result.Email);
        Assert.Equal("+91-9876543210", result.PhoneNumber);
        Assert.Equal("India", result.Country);
        Assert.Equal("Distributor", result.BusinessType);
        Assert.Equal("REG-12345", result.RegistrationNumber);
        Assert.Equal("TAX-12345", result.TaxIdentificationNumber);
        Assert.Equal("Active", result.Status);

        _partnerRepositoryMock.Verify(
            r => r.GetByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetPartnerByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _partnerRepositoryMock
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Partner?)null);

        var result = await _service.GetPartnerByIdAsync(999);

        Assert.Null(result);

        _partnerRepositoryMock.Verify(
            r => r.GetByIdAsync(999),
            Times.Once);
    }

    // =========================================================
    // GetMyPartnerAsync
    // =========================================================

    [Fact]
    public async Task GetMyPartnerAsync_ShouldReturnPartner_WhenFound()
    {
        var userId = 10;

        var partner = CreatePartner(1, "ABC Pharma");
        partner.UserId = userId;

        _partnerRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(partner);

        var result = await _service.GetMyPartnerAsync(userId);

        Assert.NotNull(result);

        Assert.Equal(1, result.Id);
        Assert.Equal("ABC Pharma", result.CompanyName);
        Assert.Equal("John Doe", result.ContactPerson);
        Assert.Equal("Active", result.Status);

        _partnerRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId),
            Times.Once);
    }

    [Fact]
    public async Task GetMyPartnerAsync_ShouldReturnNull_WhenPartnerNotFound()
    {
        var userId = 999;

        _partnerRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Partner?)null);

        var result = await _service.GetMyPartnerAsync(userId);

        Assert.Null(result);

        _partnerRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId),
            Times.Once);
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static PartnerRequest CreateRequest(
        int id,
        string companyName)
    {
        return new PartnerRequest
        {
            Id = id,
            UserId = 10,
            CompanyName = companyName,
            ContactPerson = "John Doe",
            Email = "john@abc.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "1000 units/month",
            Message = "Interested in long-term partnership.",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Partner CreatePartner(
        int id,
        string companyName)
    {
        return new Partner
        {
            Id = id,
            UserId = 10,
            CompanyName = companyName,
            ContactPerson = "John Doe",
            Email = "john@abc.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            RegistrationNumber = "REG-12345",
            TaxIdentificationNumber = "TAX-12345",
            Status = "Active",
            JoinedAt = DateTime.UtcNow
        };
    }

    private static PartnerRequestCreateDto CreateRequestDto()
    {
        return new PartnerRequestCreateDto
        {
            CompanyName = "ABC Pharma",
            ContactPerson = "John Doe",
            Email = "john@abc.com",
            PhoneNumber = "+91-9876543210",
            Country = "India",
            BusinessType = "Distributor",
            ExpectedVolume = "1000 units/month",
            Message = "Interested in long-term partnership."
        };
    }
}