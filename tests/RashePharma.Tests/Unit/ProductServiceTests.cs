using Moq;
using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAllAsync_ShouldReturnProducts()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "MEDOFCIN-200",
                Slug = "medofcin-200",
                GenericName = "Ofloxacin",
                DosageForm = "Tablet",
                Manufacturer = "Rashe Lifesciences",
                CategoryId = 1,
                IsActive = true,
                Category = new Category
                {
                    Id = 1,
                    Name = "Antibiotics",
                    Slug = "antibiotics"
                },
                Variants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        Id = 1,
                        Price = 100,
                        IsActive = true
                    }
                },
                Images = new List<ProductImage>
                {
                    new ProductImage
                    {
                        Id = 1,
                        ImageUrl = "/images/medofcin-200.jpg",
                        IsPrimary = true,
                        DisplayOrder = 1
                    }
                }
            }
        };

        productRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("MEDOFCIN-200", result[0].Name);
        Assert.Equal("medofcin-200", result[0].Slug);
        Assert.Equal("Ofloxacin", result[0].GenericName);
        Assert.Equal("Tablet", result[0].DosageForm);
        Assert.Equal("Rashe Lifesciences", result[0].Manufacturer);
        Assert.Equal("Antibiotics", result[0].CategoryName);
        Assert.Equal(100, result[0].StartingPrice);
        Assert.Equal(
            "/images/medofcin-200.jpg",
            result[0].PrimaryImageUrl);
        Assert.True(result[0].IsActive);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Antibiotic tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true,
            Category = new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            },
            Variants = new List<ProductVariant>
            {
                new ProductVariant
                {
                    Id = 1,
                    Strength = "200 mg",
                    PackSize = "10 Tablets",
                    Price = 120,
                    SKU = "MEDOFCIN-200-10",
                    StockQuantity = 50,
                    IsActive = true
                }
            },
            Images = new List<ProductImage>
            {
                new ProductImage
                {
                    Id = 1,
                    ImageUrl = "/images/medofcin-200.jpg",
                    AltText = "MEDOFCIN-200",
                    IsPrimary = true,
                    DisplayOrder = 1
                }
            }
        };

        productRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(1, result.Id);
        Assert.Equal("MEDOFCIN-200", result.Name);
        Assert.Equal("medofcin-200", result.Slug);
        Assert.Equal("Ofloxacin", result.GenericName);
        Assert.Equal("Ofloxacin 200 mg", result.Composition);
        Assert.Equal("Tablet", result.DosageForm);
        Assert.Equal("Antibiotic tablet", result.Description);
        Assert.Equal("Rashe Lifesciences", result.Manufacturer);
        Assert.Equal(1, result.CategoryId);
        Assert.Equal("Antibiotics", result.CategoryName);
        Assert.True(result.IsActive);

        Assert.Single(result.Variants);
        Assert.Equal("200 mg", result.Variants[0].Strength);
        Assert.Equal("10 Tablets", result.Variants[0].PackSize);
        Assert.Equal(120, result.Variants[0].Price);
        Assert.Equal("MEDOFCIN-200-10", result.Variants[0].SKU);
        Assert.Equal(50, result.Variants[0].StockQuantity);

        Assert.Single(result.Images);
        Assert.Equal(
            "/images/medofcin-200.jpg",
            result.Images[0].ImageUrl);
        Assert.True(result.Images[0].IsPrimary);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnProduct_WhenSlugExists()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            DosageForm = "Tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true,
            Category = new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            }
        };

        productRepository
            .Setup(r => r.GetBySlugAsync("medofcin-200"))
            .ReturnsAsync(product);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetBySlugAsync("medofcin-200");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("MEDOFCIN-200", result.Name);
        Assert.Equal("medofcin-200", result.Slug);
        Assert.Equal("Antibiotics", result.CategoryName);
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugDoesNotExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetBySlugAsync("unknown-product"))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetBySlugAsync("unknown-product");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new ProductCreateDto
        {
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Antibiotic tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(false);

        categoryRepository
            .Setup(r => r.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            });

        productRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Product
            {
                Id = id,
                Name = dto.Name,
                Slug = dto.Slug,
                GenericName = dto.GenericName,
                Composition = dto.Composition,
                DosageForm = dto.DosageForm,
                Description = dto.Description,
                Manufacturer = dto.Manufacturer,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive,
                Category = new Category
                {
                    Id = 1,
                    Name = "Antibiotics",
                    Slug = "antibiotics"
                },
                Variants = new List<ProductVariant>(),
                Images = new List<ProductImage>()
            });

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal("MEDOFCIN-200", result.Name);
        Assert.Equal("medofcin-200", result.Slug);
        Assert.Equal("Ofloxacin", result.GenericName);
        Assert.Equal("Ofloxacin 200 mg", result.Composition);
        Assert.Equal("Tablet", result.DosageForm);
        Assert.Equal("Antibiotic tablet", result.Description);
        Assert.Equal("Rashe Lifesciences", result.Manufacturer);
        Assert.Equal(1, result.CategoryId);
        Assert.True(result.IsActive);

        productRepository.Verify(
            r => r.AddAsync(It.IsAny<Product>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenSlugAlreadyExists()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new ProductCreateDto
        {
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Antibiotic tablet",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(true);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        productRepository.Verify(
            r => r.AddAsync(It.IsAny<Product>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new ProductCreateDto
        {
            Name = "Invalid Product",
            Slug = "invalid-product",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test Description",
            Manufacturer = "Test Manufacturer",
            CategoryId = 99999,
            IsActive = true
        };

        productRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(false);

        categoryRepository
            .Setup(r => r.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync((Category?)null);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        Assert.Equal("Category does not exist.", exception.Message);

        productRepository.Verify(
            r => r.AddAsync(It.IsAny<Product>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Old description",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true,
            Category = new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            }
        };

        var dto = new ProductUpdateDto
        {
            Name = "MEDOFCIN-200 Updated",
            Slug = "medofcin-200-updated",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg Updated",
            DosageForm = "Tablet",
            Description = "Updated description",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        productRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(false);

        categoryRepository
            .Setup(r => r.GetByIdAsync(dto.CategoryId))
            .ReturnsAsync(new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            });

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateAsync(1, dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal("MEDOFCIN-200 Updated", result.Name);
        Assert.Equal("medofcin-200-updated", result.Slug);
        Assert.Equal(
            "Ofloxacin 200 mg Updated",
            result.Composition);
        Assert.Equal(
            "Updated description",
            result.Description);
        Assert.True(result.IsActive);

        productRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new ProductUpdateDto
        {
            Name = "Updated Product",
            Slug = "updated-product",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Updated description",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateAsync(999, dto);

        // Assert
        Assert.Null(result);

        productRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenSlugAlreadyExists()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            CategoryId = 1,
            IsActive = true,
            Category = new Category
            {
                Id = 1,
                Name = "Antibiotics",
                Slug = "antibiotics"
            }
        };

        var dto = new ProductUpdateDto
        {
            Name = "MEDOFCIN-200 Updated",
            Slug = "painkillers",
            GenericName = "Ofloxacin",
            Composition = "Ofloxacin 200 mg",
            DosageForm = "Tablet",
            Description = "Updated description",
            Manufacturer = "Rashe Lifesciences",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        productRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(true);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(1, dto));

        productRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Product>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProductSuccessfully()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var product = new Product
        {
            Id = 1,
            Name = "MEDOFCIN-200",
            Slug = "medofcin-200",
            CategoryId = 1,
            IsActive = true
        };

        productRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result);

        productRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Product>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = new ProductService(
            productRepository.Object,
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);

        productRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Product>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }
}