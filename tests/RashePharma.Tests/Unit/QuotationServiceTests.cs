using Moq;
using RashePharma.Application.DTOs.Quotations;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class QuotationServiceTests
{
    // =========================================================
    // GetAllAsync
    // =========================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotations = new List<Quotation>
        {
            CreateQuotation(1, 10, 100, "QUO-001"),
            CreateQuotation(2, 20, 101, "QUO-002")
        };

        quotationRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(quotations);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("QUO-001", result[0].QuoteNumber);
        Assert.Equal(100, result[0].EnquiryId);
        Assert.Equal(250, result[0].TotalAmount);
        Assert.Equal("INR", result[0].Currency);
        Assert.Equal("Draft", result[0].Status);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("QUO-002", result[1].QuoteNumber);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Quotation>());

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByUserIdAsync
    // =========================================================

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotations = new List<Quotation>
        {
            CreateQuotation(1, 10, 100, "QUO-001"),
            CreateQuotation(2, 10, 101, "QUO-002")
        };

        quotationRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(quotations);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, q => Assert.Equal("INR", q.Currency));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(new List<Quotation>());

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByEnquiryIdAsync - Customer
    // =========================================================

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldReturnOnlyCurrentUsersQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotations = new List<Quotation>
        {
            CreateQuotation(1, 10, 100, "QUO-001"),
            CreateQuotation(2, 20, 100, "QUO-002")
        };

        quotationRepository
            .Setup(r => r.GetByEnquiryIdAsync(100))
            .ReturnsAsync(quotations);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetByEnquiryIdAsync(
            100,
            10);

        Assert.NotNull(result);
        Assert.Single(result);

        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldReturnEmpty_WhenUserOwnsNoQuotation()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotations = new List<Quotation>
        {
            CreateQuotation(1, 20, 100, "QUO-001")
        };

        quotationRepository
            .Setup(r => r.GetByEnquiryIdAsync(100))
            .ReturnsAsync(quotations);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetByEnquiryIdAsync(
            100,
            10);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByEnquiryIdAsync_ShouldReturnEmpty_WhenNoQuotationsExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByEnquiryIdAsync(999))
            .ReturnsAsync(new List<Quotation>());

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result = await service.GetByEnquiryIdAsync(
            999,
            10);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByEnquiryIdForAdminAsync
    // =========================================================

    [Fact]
    public async Task GetByEnquiryIdForAdminAsync_ShouldReturnAllQuotations()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotations = new List<Quotation>
        {
            CreateQuotation(1, 10, 100, "QUO-001"),
            CreateQuotation(2, 20, 100, "QUO-002")
        };

        quotationRepository
            .Setup(r => r.GetByEnquiryIdAsync(100))
            .ReturnsAsync(quotations);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryIdForAdminAsync(100);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    // =========================================================
    // GetByIdAsync - Customer
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnQuotation_WhenUserOwnsIt()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 10, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByIdAsync(1, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("QUO-001", result.QuoteNumber);
        Assert.Equal(100, result.EnquiryId);
        Assert.Equal(250, result.TotalAmount);
        Assert.Equal("INR", result.Currency);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("Quotation notes", result.Notes);

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal("MEDOFCIN-200", result.Items[0].ProductName);
        Assert.Equal("200 mg", result.Items[0].Strength);
        Assert.Equal("10 Tablets", result.Items[0].PackSize);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(125, result.Items[0].UnitPrice);
        Assert.Equal(250, result.Items[0].TotalPrice);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenQuotationBelongsToAnotherUser()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 20, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByIdAsync(1, 10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenQuotationDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Quotation?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByIdAsync(999, 10);

        Assert.Null(result);
    }

    // =========================================================
    // GetByIdForAdminAsync
    // =========================================================

    [Fact]
    public async Task GetByIdForAdminAsync_ShouldReturnAnyUsersQuotation()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 20, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByIdForAdminAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("QUO-001", result.QuoteNumber);
    }

    [Fact]
    public async Task GetByIdForAdminAsync_ShouldReturnNull_WhenQuotationDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Quotation?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByIdForAdminAsync(999);

        Assert.Null(result);
    }

    // =========================================================
    // GetByQuoteNumberAsync - Customer
    // =========================================================

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnQuotation_WhenUserOwnsIt()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 10, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByQuoteNumberAsync("QUO-001"))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByQuoteNumberAsync(
                "QUO-001",
                10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("QUO-001", result.QuoteNumber);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnNull_WhenQuotationBelongsToAnotherUser()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 20, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByQuoteNumberAsync("QUO-001"))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByQuoteNumberAsync(
                "QUO-001",
                10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_ShouldReturnNull_WhenQuotationDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByQuoteNumberAsync("QUO-999"))
            .ReturnsAsync((Quotation?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByQuoteNumberAsync(
                "QUO-999",
                10);

        Assert.Null(result);
    }

    // =========================================================
    // GetByQuoteNumberForAdminAsync
    // =========================================================

    [Fact]
    public async Task GetByQuoteNumberForAdminAsync_ShouldReturnAnyUsersQuotation()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 20, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByQuoteNumberAsync("QUO-001"))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByQuoteNumberForAdminAsync(
                "QUO-001");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetByQuoteNumberForAdminAsync_ShouldReturnNull_WhenQuotationDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByQuoteNumberAsync("QUO-999"))
            .ReturnsAsync((Quotation?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var result =
            await service.GetByQuoteNumberForAdminAsync(
                "QUO-999");

        Assert.Null(result);
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateQuotationSuccessfully()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Quotation? createdQuotation = null;

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        var variant = CreateActiveVariant(
            5,
            125,
            "MEDOFCIN-200");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(variant);

        quotationRepository
            .Setup(r => r.AddAsync(It.IsAny<Quotation>()))
            .Callback<Quotation>(quotation =>
            {
                createdQuotation = quotation;
                quotation.Id = 100;
            })
            .Returns(Task.CompletedTask);

        quotationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() => createdQuotation);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var result =
            await service.CreateAsync(dto, 10);

        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.StartsWith("QUO-", result.QuoteNumber);
        Assert.Equal(100, result.EnquiryId);

        // Price comes from ProductVariant.Price
        Assert.Equal(250, result.TotalAmount);
        Assert.Equal(125, result.Items[0].UnitPrice);
        Assert.Equal(250, result.Items[0].TotalPrice);

        Assert.Equal("INR", result.Currency);
        Assert.Equal("Draft", result.Status);
        Assert.Equal("Quotation notes", result.Notes);

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal(2, result.Items[0].Quantity);

        Assert.NotNull(createdQuotation);
        Assert.Equal(10, createdQuotation!.UserId);
        Assert.Equal(250, createdQuotation.TotalAmount);
        Assert.Single(createdQuotation.Items);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectEmptyItems()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();
        dto.Items = new List<CreateQuotationItemDto>();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Quotation must contain at least one item.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectZeroQuantity()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();
        dto.Items[0].Quantity = 0;

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Quotation item quantity must be greater than zero.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenEnquiryDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Enquiry not found.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenEnquiryBelongsToAnotherUser()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 20
        };

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "You can only create a quotation for your own enquiry.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenVariantDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync((ProductVariant?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product variant 5 not found.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenVariantIsInactive()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        var variant = CreateActiveVariant(
            5,
            125,
            "MEDOFCIN-200");

        variant.IsActive = false;

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(variant);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product variant 5 is inactive.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenProductIsInactive()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        var variant = CreateActiveVariant(
            5,
            125,
            "MEDOFCIN-200");

        variant.Product.IsActive = false;

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(variant);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product for variant 5 is inactive.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldUseDatabasePrice_AndIgnoreClientPrice()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Quotation? createdQuotation = null;

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        var variant = CreateActiveVariant(
            5,
            125,
            "MEDOFCIN-200");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(variant);

        quotationRepository
            .Setup(r => r.AddAsync(It.IsAny<Quotation>()))
            .Callback<Quotation>(quotation =>
            {
                createdQuotation = quotation;
                quotation.Id = 100;
            })
            .Returns(Task.CompletedTask);

        quotationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() => createdQuotation);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        var result =
            await service.CreateAsync(dto, 10);

        Assert.NotNull(result);

        Assert.Equal(125, result.Items[0].UnitPrice);
        Assert.Equal(250, result.Items[0].TotalPrice);
        Assert.Equal(250, result.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateQuotationWithMultipleItems()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Quotation? createdQuotation = null;

        var enquiry = new Enquiry
        {
            Id = 100,
            UserId = 10
        };

        var variant1 = CreateActiveVariant(
            5,
            125,
            "MEDOFCIN-200");

        var variant2 = CreateActiveVariant(
            6,
            50,
            "TEST-PRODUCT");

        variant2.Strength = "500 mg";

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .ReturnsAsync(enquiry);

        variantRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(variant1);

        variantRepository
            .Setup(r => r.GetByIdAsync(6))
            .ReturnsAsync(variant2);

        quotationRepository
            .Setup(r => r.AddAsync(It.IsAny<Quotation>()))
            .Callback<Quotation>(quotation =>
            {
                createdQuotation = quotation;
                quotation.Id = 102;
            })
            .Returns(Task.CompletedTask);

        quotationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() => createdQuotation);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();

        dto.Items.Add(new CreateQuotationItemDto
        {
            ProductVariantId = 6,
            Quantity = 3
        });

        var result =
            await service.CreateAsync(dto, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(250, result.Items[0].TotalPrice);
        Assert.Equal(150, result.Items[1].TotalPrice);
        Assert.Equal(400, result.TotalAmount);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectPastValidUntil()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();
        dto.ValidUntil = DateTime.UtcNow.AddDays(-1);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Quotation validity date must be in the future.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectBlankCurrency()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();
        dto.Currency = " ";

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Currency is required.",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectUnsupportedCurrency()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = CreateQuotationDto();
        dto.Currency = "XYZ";

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Unsupported currency: XYZ",
            exception.Message);

        quotationRepository.Verify(
            r => r.AddAsync(It.IsAny<Quotation>()),
            Times.Never);
    }

    // =========================================================
    // UpdateStatusAsync
    // =========================================================

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatusSuccessfully()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var quotation =
            CreateQuotation(1, 10, 100, "QUO-001");

        quotationRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(quotation);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = new UpdateQuotationStatusDto
        {
            Status = "sent"
        };

        var result =
            await service.UpdateStatusAsync(1, dto);

        Assert.True(result);
        Assert.Equal("sent", quotation.Status);
        Assert.NotNull(quotation.UpdatedAt);

        quotationRepository.Verify(
            r => r.UpdateAsync(quotation),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldRejectInvalidStatus()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = new UpdateQuotationStatusDto
        {
            Status = "InvalidStatus"
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateStatusAsync(1, dto));

        Assert.Equal(
            "Invalid quotation status: InvalidStatus",
            exception.Message);

        quotationRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        quotationRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldRejectBlankStatus()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = new UpdateQuotationStatusDto
        {
            Status = " "
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateStatusAsync(1, dto));

        Assert.Equal(
            "Quotation status is required.",
            exception.Message);

        quotationRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        quotationRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnFalse_WhenQuotationDoesNotExist()
    {
        var quotationRepository = new Mock<IQuotationRepository>();
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var variantRepository = new Mock<IProductVariantRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        quotationRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Quotation?)null);

        var service = CreateService(
            quotationRepository,
            enquiryRepository,
            variantRepository,
            unitOfWork);

        var dto = new UpdateQuotationStatusDto
        {
            Status = "sent"
        };

        var result =
            await service.UpdateStatusAsync(999, dto);

        Assert.False(result);

        quotationRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Quotation>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static QuotationService CreateService(
        Mock<IQuotationRepository> quotationRepository,
        Mock<IEnquiryRepository> enquiryRepository,
        Mock<IProductVariantRepository> variantRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new QuotationService(
            quotationRepository.Object,
            enquiryRepository.Object,
            variantRepository.Object,
            unitOfWork.Object);
    }

    private static ProductVariant CreateActiveVariant(
        int id,
        decimal price,
        string productName)
    {
        return new ProductVariant
        {
            Id = id,
            Price = price,
            IsActive = true,
            Strength = "200 mg",
            PackSize = "10 Tablets",

            Product = new Product
            {
                Id = id,
                Name = productName,
                IsActive = true
            }
        };
    }

    private static Quotation CreateQuotation(
        int id,
        int userId,
        int enquiryId,
        string quoteNumber)
    {
        return new Quotation
        {
            Id = id,
            EnquiryId = enquiryId,
            UserId = userId,
            QuoteNumber = quoteNumber,
            TotalAmount = 250,
            Currency = "INR",
            Status = "Draft",
            Notes = "Quotation notes",
            ValidUntil = DateTime.UtcNow.AddDays(30),

            Items = new List<QuotationItem>
            {
                new QuotationItem
                {
                    Id = 1,
                    ProductVariantId = 5,
                    Quantity = 2,
                    UnitPrice = 125,
                    TotalPrice = 250,

                    ProductVariant = new ProductVariant
                    {
                        Id = 5,
                        Price = 125,
                        Strength = "200 mg",
                        PackSize = "10 Tablets",

                        Product = new Product
                        {
                            Id = 1,
                            Name = "MEDOFCIN-200",
                            IsActive = true
                        }
                    }
                }
            }
        };
    }

    private static CreateQuotationDto CreateQuotationDto()
    {
        return new CreateQuotationDto
        {
            EnquiryId = 100,
            Currency = "INR",
            ValidUntil = DateTime.UtcNow.AddDays(30),
            Notes = "Quotation notes",

            Items = new List<CreateQuotationItemDto>
            {
                new CreateQuotationItemDto
                {
                    ProductVariantId = 5,
                    Quantity = 2
                }
            }
        };
    }
}