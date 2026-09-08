using Moq;
using RashePharma.Application.DTOs.Addresses;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class AddressServiceTests
{
    // =========================================================
    // GetByUserIdAsync
    // =========================================================

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnUserAddresses()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var addresses = new List<Address>
        {
            new Address
            {
                Id = 1,
                UserId = 10,
                AddressLine1 = "123 MG Road",
                AddressLine2 = "Near Metro Station",
                City = "Bengaluru",
                State = "Karnataka",
                PostalCode = "560001",
                Country = "India",
                AddressType = "Home",
                IsDefault = true
            },
            new Address
            {
                Id = 2,
                UserId = 10,
                AddressLine1 = "456 Residency Road",
                AddressLine2 = null,
                City = "Bengaluru",
                State = "Karnataka",
                PostalCode = "560025",
                Country = "India",
                AddressType = "Office",
                IsDefault = false
            }
        };

        addressRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(addresses);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByUserIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal("123 MG Road", result[0].AddressLine1);
        Assert.Equal("Bengaluru", result[0].City);
        Assert.Equal("Karnataka", result[0].State);
        Assert.Equal("560001", result[0].PostalCode);
        Assert.Equal("India", result[0].Country);
        Assert.Equal("Home", result[0].AddressType);
        Assert.True(result[0].IsDefault);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("456 Residency Road", result[1].AddressLine1);
        Assert.Equal("Office", result[1].AddressType);
        Assert.False(result[1].IsDefault);
    }


    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenNoAddressesExist()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        addressRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(new List<Address>());

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByUserIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAddress_WhenAddressBelongsToUser()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 10,
            AddressLine1 = "123 MG Road",
            AddressLine2 = "Near Metro Station",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByIdAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("123 MG Road", result.AddressLine1);
        Assert.Equal("Near Metro Station", result.AddressLine2);
        Assert.Equal("Bengaluru", result.City);
        Assert.Equal("Karnataka", result.State);
        Assert.Equal("560001", result.PostalCode);
        Assert.Equal("India", result.Country);
        Assert.Equal("Home", result.AddressType);
        Assert.True(result.IsDefault);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAddressDoesNotExist()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        addressRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Address?)null);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByIdAsync(999, 10);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 20,
            AddressLine1 = "123 MG Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetByIdAsync(1, 10);

        // Assert
        Assert.Null(result);
    }


    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateAddressSuccessfully()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new AddressCreateDto
        {
            AddressLine1 = "123 MG Road",
            AddressLine2 = "Near Metro Station",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.AddAsync(It.IsAny<Address>()))
            .Callback<Address>(address => address.Id = 1)
            .Returns(Task.CompletedTask);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.CreateAsync(10, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("123 MG Road", result.AddressLine1);
        Assert.Equal("Near Metro Station", result.AddressLine2);
        Assert.Equal("Bengaluru", result.City);
        Assert.Equal("Karnataka", result.State);
        Assert.Equal("560001", result.PostalCode);
        Assert.Equal("India", result.Country);
        Assert.Equal("Home", result.AddressType);
        Assert.True(result.IsDefault);

        addressRepository.Verify(
            r => r.AddAsync(It.IsAny<Address>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    [Fact]
    public async Task CreateAsync_ShouldAssignCorrectUserId()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new AddressCreateDto
        {
            AddressLine1 = "123 MG Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        Address? capturedAddress = null;

        addressRepository
            .Setup(r => r.AddAsync(It.IsAny<Address>()))
            .Callback<Address>(address =>
            {
                capturedAddress = address;
                address.Id = 1;
            })
            .Returns(Task.CompletedTask);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        await service.CreateAsync(25, dto);

        // Assert
        Assert.NotNull(capturedAddress);
        Assert.Equal(25, capturedAddress!.UserId);
    }


    [Fact]
    public async Task CreateAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new AddressCreateDto
        {
            AddressLine1 = "10 Residency Road",
            AddressLine2 = "Floor 2",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560025",
            Country = "India",
            AddressType = "Office",
            IsDefault = false
        };

        Address? capturedAddress = null;

        addressRepository
            .Setup(r => r.AddAsync(It.IsAny<Address>()))
            .Callback<Address>(address =>
            {
                capturedAddress = address;
                address.Id = 5;
            })
            .Returns(Task.CompletedTask);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        await service.CreateAsync(10, dto);

        // Assert
        Assert.NotNull(capturedAddress);

        Assert.Equal(dto.AddressLine1, capturedAddress!.AddressLine1);
        Assert.Equal(dto.AddressLine2, capturedAddress.AddressLine2);
        Assert.Equal(dto.City, capturedAddress.City);
        Assert.Equal(dto.State, capturedAddress.State);
        Assert.Equal(dto.PostalCode, capturedAddress.PostalCode);
        Assert.Equal(dto.Country, capturedAddress.Country);
        Assert.Equal(dto.AddressType, capturedAddress.AddressType);
        Assert.Equal(dto.IsDefault, capturedAddress.IsDefault);
    }


    // =========================================================
    // UpdateAsync
    // =========================================================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAddressSuccessfully()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 10,
            AddressLine1 = "Old Address",
            AddressLine2 = "Old Line 2",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        var dto = new AddressUpdateDto
        {
            AddressLine1 = "Updated Address",
            AddressLine2 = "Updated Line 2",
            City = "Mysuru",
            State = "Karnataka",
            PostalCode = "570001",
            Country = "India",
            AddressType = "Office",
            IsDefault = false
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateAsync(1, 10, dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal("Updated Address", result.AddressLine1);
        Assert.Equal("Updated Line 2", result.AddressLine2);
        Assert.Equal("Mysuru", result.City);
        Assert.Equal("Karnataka", result.State);
        Assert.Equal("570001", result.PostalCode);
        Assert.Equal("India", result.Country);
        Assert.Equal("Office", result.AddressType);
        Assert.False(result.IsDefault);

        addressRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Address>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenAddressDoesNotExist()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var dto = new AddressUpdateDto
        {
            AddressLine1 = "Updated Address",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Address?)null);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateAsync(999, 10, dto);

        // Assert
        Assert.Null(result);

        addressRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Address>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 20,
            AddressLine1 = "Original Address",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        var dto = new AddressUpdateDto
        {
            AddressLine1 = "Updated Address",
            City = "Mysuru",
            State = "Karnataka",
            PostalCode = "570001",
            Country = "India",
            AddressType = "Office",
            IsDefault = false
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateAsync(1, 10, dto);

        // Assert
        Assert.Null(result);

        addressRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Address>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    // =========================================================
    // DeleteAsync
    // =========================================================

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAddressSuccessfully()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 10,
            AddressLine1 = "123 MG Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.DeleteAsync(1, 10);

        // Assert
        Assert.True(result);

        addressRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Address>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }


    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAddressDoesNotExist()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        addressRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Address?)null);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.DeleteAsync(999, 10);

        // Assert
        Assert.False(result);

        addressRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Address>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }


    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenAddressBelongsToAnotherUser()
    {
        // Arrange
        var addressRepository = new Mock<IAddressRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var address = new Address
        {
            Id = 1,
            UserId = 20,
            AddressLine1 = "123 MG Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        var service = new AddressService(
            addressRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.DeleteAsync(1, 10);

        // Assert
        Assert.False(result);

        addressRepository.Verify(
            r => r.DeleteAsync(It.IsAny<Address>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }
}