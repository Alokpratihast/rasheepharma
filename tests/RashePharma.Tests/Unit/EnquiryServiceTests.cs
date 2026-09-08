using Moq;
using RashePharma.Application.DTOs.Enquiries;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class EnquiryServiceTests
{
    // =========================================================
    // GetAllAsync
    // =========================================================

    [Fact]
    public async Task GetAllAsync_ShouldReturnEnquiries()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiries = new List<Enquiry>
        {
            CreateEnquiry(1, 10, "ENQ-001"),
            CreateEnquiry(2, 20, "ENQ-002")
        };

        enquiryRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(enquiries);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("ENQ-001", result[0].EnquiryNumber);
        Assert.Equal("John Doe", result[0].CustomerName);
        Assert.Equal("john@example.com", result[0].Email);
        Assert.Equal("India", result[0].Country);
        Assert.Equal("Pharmacy", result[0].BusinessType);
        Assert.Equal("Pending", result[0].Status);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoEnquiries()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Enquiry>());

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }


    // =========================================================
    // GetByUserIdAsync
    // =========================================================

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserEnquiries()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiries = new List<Enquiry>
        {
            CreateEnquiry(1, 10, "ENQ-001"),
            CreateEnquiry(2, 10, "ENQ-002")
        };

        enquiryRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(enquiries);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal("India", e.Country));
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmpty_WhenUserHasNoEnquiries()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(new List<Enquiry>());

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByUserIdAsync(10);

        Assert.NotNull(result);
        Assert.Empty(result);
    }


    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEnquiry_WhenUserOwnsEnquiry()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByIdAsync(1, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ENQ-001", result.EnquiryNumber);
        Assert.Equal("John Doe", result.CustomerName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("9876543210", result.PhoneNumber);
        Assert.Equal("India", result.Country);
        Assert.Equal("Pharmacy", result.BusinessType);
        Assert.Equal("Need pharmaceutical products.", result.Message);
        Assert.Equal("Pending", result.Status);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEnquiryDoesNotExist()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByIdAsync(999, 10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEnquiryBelongsToAnotherUser()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        // Enquiry belongs to user 10,
        // but user 20 is trying to access it.
        var result = await service.GetByIdAsync(1, 20);

        Assert.Null(result);
    }


    // =========================================================
    // GetByEnquiryNumberAsync
    // =========================================================

    [Fact]
    public async Task GetByEnquiryNumberAsync_ShouldReturnEnquiry_WhenUserOwnsEnquiry()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByEnquiryNumberAsync("ENQ-001"))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryNumberAsync(
                "ENQ-001",
                10);

        Assert.NotNull(result);
        Assert.Equal("ENQ-001", result.EnquiryNumber);
        Assert.Equal(1, result.Id);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByEnquiryNumberAsync_ShouldReturnNull_WhenEnquiryDoesNotExist()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByEnquiryNumberAsync("ENQ-999"))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryNumberAsync(
                "ENQ-999",
                10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEnquiryNumberAsync_ShouldReturnNull_WhenEnquiryBelongsToAnotherUser()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByEnquiryNumberAsync("ENQ-001"))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryNumberAsync(
                "ENQ-001",
                20);

        Assert.Null(result);
    }


    // =========================================================
    // GetByIdForAdminAsync
    // =========================================================

    [Fact]
    public async Task GetByIdForAdminAsync_ShouldReturnEnquiry_WhenFound()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByIdForAdminAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ENQ-001", result.EnquiryNumber);
    }

    [Fact]
    public async Task GetByIdForAdminAsync_ShouldReturnNull_WhenNotFound()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByIdForAdminAsync(999);

        Assert.Null(result);
    }


    // =========================================================
    // GetByEnquiryNumberForAdminAsync
    // =========================================================

    [Fact]
    public async Task GetByEnquiryNumberForAdminAsync_ShouldReturnEnquiry_WhenFound()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByEnquiryNumberAsync("ENQ-001"))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryNumberForAdminAsync(
                "ENQ-001");

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ENQ-001", result.EnquiryNumber);
    }

    [Fact]
    public async Task GetByEnquiryNumberForAdminAsync_ShouldReturnNull_WhenNotFound()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByEnquiryNumberAsync("ENQ-999"))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.GetByEnquiryNumberForAdminAsync(
                "ENQ-999");

        Assert.Null(result);
    }


    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateEnquirySuccessfully_ForLoggedInUser()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Enquiry? createdEnquiry = null;

        var variant = CreateProductVariant(
            5,
            isVariantActive: true,
            isProductActive: true);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        enquiryRepository
            .Setup(r => r.AddAsync(It.IsAny<Enquiry>()))
            .Callback<Enquiry>(enquiry =>
{
    createdEnquiry = enquiry;
    enquiry.Id = 100;

    foreach (var item in enquiry.Items)
    {
        item.ProductVariant = CreateProductVariant(
            item.ProductVariantId,
            isVariantActive: true,
            isProductActive: true);
    }
})
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        enquiryRepository
            .Setup(r => r.GetByIdAsync(100))
            .Returns(() => Task.FromResult(createdEnquiry));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();

        var result =
            await service.CreateAsync(dto, 10);

        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.StartsWith("ENQ-", result.EnquiryNumber);
        Assert.Equal("John Doe", result.CustomerName);
        Assert.Equal("john@example.com", result.Email);
        Assert.Equal("9876543210", result.PhoneNumber);
        Assert.Equal("India", result.Country);
        Assert.Equal("Pharmacy", result.BusinessType);
        Assert.Equal("Need pharmaceutical products.", result.Message);
        Assert.Equal("Pending", result.Status);

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(
            "Please quote this product.",
            result.Items[0].Message);

        Assert.NotNull(createdEnquiry);
        Assert.Equal(10, createdEnquiry!.UserId);
        Assert.Single(createdEnquiry.Items);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(5),
            Times.Once);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    [Fact]
    public async Task CreateAsync_ShouldCreateEnquirySuccessfully_ForGuestUser()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Enquiry? createdEnquiry = null;

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(
                CreateProductVariant(
                    5,
                    isVariantActive: true,
                    isProductActive: true));

        enquiryRepository
            .Setup(r => r.AddAsync(It.IsAny<Enquiry>()))
            .Callback<Enquiry>(enquiry =>
{
    createdEnquiry = enquiry;
    enquiry.Id = 101;

    foreach (var item in enquiry.Items)
    {
        item.ProductVariant = CreateProductVariant(
            item.ProductVariantId,
            isVariantActive: true,
            isProductActive: true);
    }
})
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        enquiryRepository
            .Setup(r => r.GetByIdAsync(101))
            .Returns(() => Task.FromResult(createdEnquiry));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var result =
            await service.CreateAsync(
                CreateEnquiryDto(),
                null);

        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.StartsWith("ENQ-", result.EnquiryNumber);
        Assert.Equal("Pending", result.Status);

        Assert.NotNull(createdEnquiry);
        Assert.Null(createdEnquiry!.UserId);
    }


    [Fact]
    public async Task CreateAsync_ShouldCreateEnquiryWithMultipleItems()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Enquiry? createdEnquiry = null;

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(
                CreateProductVariant(
                    5,
                    isVariantActive: true,
                    isProductActive: true));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(6))
            .ReturnsAsync(
                CreateProductVariant(
                    6,
                    isVariantActive: true,
                    isProductActive: true));

        enquiryRepository
            .Setup(r => r.AddAsync(It.IsAny<Enquiry>()))
            .Callback<Enquiry>(enquiry =>
{
    createdEnquiry = enquiry;
    enquiry.Id = 102;

    foreach (var item in enquiry.Items)
    {
        item.ProductVariant = CreateProductVariant(
            item.ProductVariantId,
            isVariantActive: true,
            isProductActive: true);
    }
})
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        enquiryRepository
            .Setup(r => r.GetByIdAsync(102))
            .Returns(() => Task.FromResult(createdEnquiry));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();

        dto.Items.Add(
            new CreateEnquiryItemDto
            {
                ProductVariantId = 6,
                Quantity = 5,
                Message = "Need bulk quotation."
            });

        var result =
            await service.CreateAsync(dto, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);

        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal(2, result.Items[0].Quantity);

        Assert.Equal(6, result.Items[1].ProductVariantId);
        Assert.Equal(5, result.Items[1].Quantity);

        Assert.NotNull(createdEnquiry);
        Assert.Equal(2, createdEnquiry!.Items.Count);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(5),
            Times.Once);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(6),
            Times.Once);
    }


    [Fact]
    public async Task CreateAsync_ShouldCreateEnquiryWithNoItems()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Enquiry? createdEnquiry = null;

        enquiryRepository
            .Setup(r => r.AddAsync(It.IsAny<Enquiry>()))
            .Callback<Enquiry>(enquiry =>
            {
                createdEnquiry = enquiry;
                enquiry.Id = 103;
            })
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        enquiryRepository
            .Setup(r => r.GetByIdAsync(103))
            .Returns(() => Task.FromResult(createdEnquiry));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();
        dto.Items = new List<CreateEnquiryItemDto>();

        var result =
            await service.CreateAsync(dto, null);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal("Pending", result.Status);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    // =========================================================
    // CreateAsync - Validation
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenQuantityIsZero()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();
        dto.Items[0].Quantity = 0;

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Enquiry item quantity must be greater than zero.",
            exception.Message);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenQuantityIsNegative()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();
        dto.Items[0].Quantity = -5;

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Enquiry item quantity must be greater than zero.",
            exception.Message);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductVariantDoesNotExist()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetVariantByIdAsync(999))
            .ReturnsAsync((ProductVariant?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();
        dto.Items[0].ProductVariantId = 999;

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product variant with id 999 was not found.",
            exception.Message);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductVariantIsInactive()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(
                CreateProductVariant(
                    5,
                    isVariantActive: false,
                    isProductActive: true));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product variant with id 5 is inactive.",
            exception.Message);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductIsInactive()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(
                CreateProductVariant(
                    5,
                    isVariantActive: true,
                    isProductActive: false));

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = CreateEnquiryDto();

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(dto, 10));

        Assert.Equal(
            "Product for variant 5 is inactive.",
            exception.Message);

        enquiryRepository.Verify(
            r => r.AddAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    // =========================================================
    // UpdateStatusAsync
    // =========================================================

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatusSuccessfully()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted"
        };

        var result =
            await service.UpdateStatusAsync(1, dto);

        Assert.True(result);
        Assert.Equal("Contacted", enquiry.Status);
        Assert.NotNull(enquiry.UpdatedAt);

        enquiryRepository.Verify(
            r => r.UpdateAsync(enquiry),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnFalse_WhenEnquiryDoesNotExist()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        enquiryRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Enquiry?)null);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "Contacted"
        };

        var result =
            await service.UpdateStatusAsync(999, dto);

        Assert.False(result);

        enquiryRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenStatusIsEmpty()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = ""
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateStatusAsync(1, dto));

        Assert.Equal(
            "Enquiry status is required.",
            exception.Message);

        enquiryRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenStatusIsInvalid()
    {
        var enquiryRepository = new Mock<IEnquiryRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var enquiry = CreateEnquiry(1, 10, "ENQ-001");

        enquiryRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(enquiry);

        var service = CreateService(
            enquiryRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateEnquiryStatusDto
        {
            Status = "InvalidStatus"
        };

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateStatusAsync(1, dto));

        Assert.Equal(
            "Invalid enquiry status 'InvalidStatus'.",
            exception.Message);

        enquiryRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Enquiry>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    // =========================================================
    // Helpers
    // =========================================================

    private static EnquiryService CreateService(
        Mock<IEnquiryRepository> enquiryRepository,
        Mock<IProductRepository> productRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new EnquiryService(
            enquiryRepository.Object,
            productRepository.Object,
            unitOfWork.Object);
    }


    private static ProductVariant CreateProductVariant(
        int id,
        bool isVariantActive,
        bool isProductActive)
    {
        return new ProductVariant
        {
            Id = id,
            ProductId = id,
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = 100m,
            SKU = $"SKU-{id}",
            StockQuantity = 1000,
            IsActive = isVariantActive,
            Product = new Product
            {
                Id = id,
                Name = $"Test Product {id}",
                Slug = $"test-product-{id}",
                GenericName = "Test Generic",
                Composition = "Test Composition",
                DosageForm = "Tablet",
                Description = "Test Product",
                Manufacturer = "Rashe Pharma",
                IsActive = isProductActive
            }
        };
    }


    private static Enquiry CreateEnquiry(
        int id,
        int userId,
        string enquiryNumber)
    {
        return new Enquiry
        {
            Id = id,
            UserId = userId,
            EnquiryNumber = enquiryNumber,
            CustomerName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Country = "India",
            BusinessType = "Pharmacy",
            Message = "Need pharmaceutical products.",
            Status = "Pending",

            Items = new List<EnquiryItem>
            {
                new EnquiryItem
                {
                    Id = 1,
                    ProductVariantId = 5,
                    Quantity = 2,
                    Message = "Please quote this product.",

                    ProductVariant = new ProductVariant
                    {
                        Id = 5,
                        Strength = "200 mg",
                        PackSize = "10 Tablets",

                        Product = new Product
                        {
                            Id = 1,
                            Name = "MEDOFCIN-200"
                        }
                    }
                }
            }
        };
    }


    private static CreateEnquiryDto CreateEnquiryDto()
    {
        return new CreateEnquiryDto
        {
            CustomerName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "9876543210",
            Country = "India",
            BusinessType = "Pharmacy",
            Message = "Need pharmaceutical products.",

            Items = new List<CreateEnquiryItemDto>
            {
                new CreateEnquiryItemDto
                {
                    ProductVariantId = 5,
                    Quantity = 2,
                    Message = "Please quote this product."
                }
            }
        };
    }
}