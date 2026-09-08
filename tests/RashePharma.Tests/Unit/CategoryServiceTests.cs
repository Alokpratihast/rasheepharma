using Moq;
using RashePharma.Application.DTOs.Categories;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateCategorySuccessfully()
    {
        // Arrange
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new CategoryCreateDto
        {
            Name = "Antibiotics",
            Slug = "antibiotics",
            Description = "Antibiotic pharmaceutical products",
            IsActive = true
        };

        categoryRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(false);

        var service = new CategoryService(
            categoryRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Antibiotics", result.Name);
        Assert.Equal("antibiotics", result.Slug);
        Assert.True(result.IsActive);

        categoryRepository.Verify(
            r => r.AddAsync(It.IsAny<Category>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenSlugAlreadyExists()
    {
        // Arrange
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new CategoryCreateDto
        {
            Name = "Antibiotics",
            Slug = "antibiotics",
            Description = "Antibiotic pharmaceutical products",
            IsActive = true
        };

        categoryRepository
            .Setup(r => r.ExistsBySlugAsync(dto.Slug))
            .ReturnsAsync(true);

        var service = new CategoryService(
            categoryRepository.Object,
            unitOfWork.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(dto));

        categoryRepository.Verify(
            r => r.AddAsync(It.IsAny<Category>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var category = new Category
    {
        Id = 1,
        Name = "Antibiotics",
        Slug = "antibiotics",
        Description = "Antibiotic pharmaceutical products",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(category);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("Antibiotics", result.Name);
    Assert.Equal("antibiotics", result.Slug);
}

[Fact]
public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    categoryRepository
        .Setup(r => r.GetByIdAsync(999))
        .ReturnsAsync((Category?)null);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.GetByIdAsync(999);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task GetBySlugAsync_ShouldReturnCategory_WhenSlugExists()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var category = new Category
    {
        Id = 1,
        Name = "Antibiotics",
        Slug = "antibiotics",
        Description = "Antibiotic pharmaceutical products",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetBySlugAsync("antibiotics"))
        .ReturnsAsync(category);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.GetBySlugAsync("antibiotics");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("Antibiotics", result.Name);
    Assert.Equal("antibiotics", result.Slug);
}

[Fact]
public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugDoesNotExist()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    categoryRepository
        .Setup(r => r.GetBySlugAsync("unknown-category"))
        .ReturnsAsync((Category?)null);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.GetBySlugAsync("unknown-category");

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task UpdateAsync_ShouldUpdateCategorySuccessfully()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var category = new Category
    {
        Id = 1,
        Name = "Antibiotics",
        Slug = "antibiotics",
        Description = "Old description",
        IsActive = true
    };

    var dto = new CategoryUpdateDto
    {
        Name = "Updated Antibiotics",
        Slug = "updated-antibiotics",
        Description = "Updated description",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(category);

    categoryRepository
        .Setup(r => r.ExistsBySlugAsync(dto.Slug))
        .ReturnsAsync(false);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.UpdateAsync(1, dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Updated Antibiotics", result.Name);
    Assert.Equal("updated-antibiotics", result.Slug);
    Assert.Equal("Updated description", result.Description);
    Assert.True(result.IsActive);

    categoryRepository.Verify(
        r => r.UpdateAsync(It.IsAny<Category>()),
        Times.Once);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Once);
}

[Fact]
public async Task UpdateAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var dto = new CategoryUpdateDto
    {
        Name = "Updated Antibiotics",
        Slug = "updated-antibiotics",
        Description = "Updated description",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetByIdAsync(999))
        .ReturnsAsync((Category?)null);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.UpdateAsync(999, dto);

    // Assert
    Assert.Null(result);

    categoryRepository.Verify(
        r => r.UpdateAsync(It.IsAny<Category>()),
        Times.Never);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Never);
}

[Fact]
public async Task UpdateAsync_ShouldThrowException_WhenSlugAlreadyExists()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var category = new Category
    {
        Id = 1,
        Name = "Antibiotics",
        Slug = "antibiotics",
        Description = "Old description",
        IsActive = true
    };

    var dto = new CategoryUpdateDto
    {
        Name = "Updated Antibiotics",
        Slug = "painkillers",
        Description = "Updated description",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(category);

    categoryRepository
        .Setup(r => r.ExistsBySlugAsync(dto.Slug))
        .ReturnsAsync(true);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.UpdateAsync(1, dto));

    categoryRepository.Verify(
        r => r.UpdateAsync(It.IsAny<Category>()),
        Times.Never);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Never);
}

[Fact]
public async Task DeleteAsync_ShouldDeleteCategorySuccessfully()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    var category = new Category
    {
        Id = 1,
        Name = "Antibiotics",
        Slug = "antibiotics",
        Description = "Antibiotic pharmaceutical products",
        IsActive = true
    };

    categoryRepository
        .Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(category);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.DeleteAsync(1);

    // Assert
    Assert.True(result);

    categoryRepository.Verify(
        r => r.DeleteAsync(It.IsAny<Category>()),
        Times.Once);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Once);
}

[Fact]
public async Task DeleteAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
{
    // Arrange
    var categoryRepository = new Mock<ICategoryRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();

    categoryRepository
        .Setup(r => r.GetByIdAsync(999))
        .ReturnsAsync((Category?)null);

    var service = new CategoryService(
        categoryRepository.Object,
        unitOfWork.Object);

    // Act
    var result = await service.DeleteAsync(999);

    // Assert
    Assert.False(result);

    categoryRepository.Verify(
        r => r.DeleteAsync(It.IsAny<Category>()),
        Times.Never);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Never);
}


}