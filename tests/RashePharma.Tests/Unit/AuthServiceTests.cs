using Microsoft.AspNetCore.Identity;
using Moq;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class AuthServiceTests
{
    // =========================================================
    // RegisterAsync Tests
    // =========================================================

    [Fact]
    public async Task RegisterAsync_ShouldRegisterUserSuccessfully()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var dto = new RegisterDto
        {
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PhoneNumber = "9876543210",
            Country = "India",
            Password = "Password123"
        };

        User? capturedUser = null;

        // First call:
        // User does not exist.
        //
        // Second call:
        // Return the user that was created by AddAsync().
        userRepository
            .SetupSequence(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null)
            .ReturnsAsync(() => capturedUser);

        userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                capturedUser = user;

                user.Id = 1;

                user.Role = new Role
                {
                    Id = 2,
                    Name = "Customer"
                };
            })
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("Alok", result.FirstName);
        Assert.Equal("Pratihast", result.LastName);
        Assert.Equal("alok@example.com", result.Email);
        Assert.Equal("test-jwt-token", result.Token);

        userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);

        jwtTokenService.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Once);
    }


    [Fact]
public async Task RegisterAsync_ShouldCreateUserWithDefaultRoleAndActiveStatus()
{
    // Arrange
    var userRepository = new Mock<IUserRepository>();
    var unitOfWork = new Mock<IUnitOfWork>();
    var jwtTokenService = CreateJwtTokenServiceMock();

    var dto = new RegisterDto
    {
        FirstName = "Alok",
        LastName = "Pratihast",
        Email = "alok@example.com",
        PhoneNumber = "9876543210",
        Country = "India",
        Password = "Password123"
    };

    User? capturedUser = null;

    // First call = user does not exist.
    // Second call = return the newly created user.
    userRepository
        .SetupSequence(r => r.GetByEmailAsync(dto.Email))
        .ReturnsAsync((User?)null)
        .ReturnsAsync(() => capturedUser);

    userRepository
        .Setup(r => r.AddAsync(It.IsAny<User>()))
        .Callback<User>(user =>
        {
            capturedUser = user;
            user.Id = 1;
        })
        .Returns(Task.CompletedTask);

    var service = new AuthService(
        userRepository.Object,
        unitOfWork.Object,
        jwtTokenService.Object);

    // Act
    await service.RegisterAsync(dto);

    // Assert
    Assert.NotNull(capturedUser);

    // RoleId 1 = User
    Assert.Equal(1, capturedUser!.RoleId);

    // New users should be active
    Assert.True(capturedUser.IsActive);

    userRepository.Verify(
        r => r.AddAsync(It.IsAny<User>()),
        Times.Once);

    unitOfWork.Verify(
        u => u.SaveChangesAsync(),
        Times.Once);
}


    [Fact]
    public async Task RegisterAsync_ShouldHashPassword()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var dto = new RegisterDto
        {
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PhoneNumber = "9876543210",
            Country = "India",
            Password = "Password123"
        };

        User? capturedUser = null;

        // First call = user does not exist.
        // Second call = return the newly created user.
        userRepository
            .SetupSequence(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null)
            .ReturnsAsync(() => capturedUser);

        userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                capturedUser = user;

                user.Id = 1;

                user.Role = new Role
                {
                    Id = 2,
                    Name = "Customer"
                };
            })
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        await service.RegisterAsync(dto);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotNull(capturedUser!.PasswordHash);
        Assert.NotEmpty(capturedUser.PasswordHash);

        // Password should never be stored as plain text.
        Assert.NotEqual(dto.Password, capturedUser.PasswordHash);

        // Verify that the stored hash can actually validate
        // the original password.
        var passwordHasher = new PasswordHasher<User>();

        var verificationResult = passwordHasher.VerifyHashedPassword(
            capturedUser,
            capturedUser.PasswordHash,
            dto.Password);

        Assert.Equal(
            PasswordVerificationResult.Success,
            verificationResult);
    }


    // =========================================================
    // LoginAsync Tests
    // =========================================================

    [Fact]
    public async Task LoginAsync_ShouldLoginSuccessfully_WithCorrectCredentials()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var password = "Password123";

        var passwordHasher = new PasswordHasher<User>();

        var hash = passwordHasher.HashPassword(
            new User(),
            password);

        var user = new User
        {
            Id = 1,
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PasswordHash = hash,
            IsActive = true
        };

        var dto = new LoginDto
        {
            Email = "alok@example.com",
            Password = password
        };

        userRepository
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal("Alok", result.FirstName);
        Assert.Equal("Pratihast", result.LastName);
        Assert.Equal("alok@example.com", result.Email);
        Assert.Equal("test-jwt-token", result.Token);

        jwtTokenService.Verify(
            j => j.GenerateToken(user),
            Times.Once);
    }


    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var dto = new LoginDto
        {
            Email = "unknown@example.com",
            Password = "Password123"
        };

        userRepository
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        Assert.Null(result);

        jwtTokenService.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }


    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var password = "Password123";

        var passwordHasher = new PasswordHasher<User>();

        var hash = passwordHasher.HashPassword(
            new User(),
            password);

        var user = new User
        {
            Id = 1,
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PasswordHash = hash,
            IsActive = false
        };

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = password
        };

        userRepository
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        Assert.Null(result);

        jwtTokenService.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }


    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var correctPassword = "Password123";

        var passwordHasher = new PasswordHasher<User>();

        var hash = passwordHasher.HashPassword(
            new User(),
            correctPassword);

        var user = new User
        {
            Id = 1,
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PasswordHash = hash,
            IsActive = true
        };

        var dto = new LoginDto
        {
            Email = user.Email,
            Password = "WrongPassword"
        };

        userRepository
            .Setup(r => r.GetByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.LoginAsync(dto);

        // Assert
        Assert.Null(result);

        jwtTokenService.Verify(
            j => j.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }


    // =========================================================
    // GetProfileAsync Tests
    // =========================================================

    [Fact]
    public async Task GetProfileAsync_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var user = new User
        {
            Id = 1,
            FirstName = "Alok",
            LastName = "Pratihast",
            Email = "alok@example.com",
            PhoneNumber = "9876543210",
            Country = "India",
            IsActive = true
        };

        userRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.GetProfileAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Alok", result.FirstName);
        Assert.Equal("Pratihast", result.LastName);
        Assert.Equal("alok@example.com", result.Email);
        Assert.Equal("9876543210", result.PhoneNumber);
        Assert.Equal("India", result.Country);
    }


    [Fact]
    public async Task GetProfileAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        userRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((User?)null);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.GetProfileAsync(999);

        // Assert
        Assert.Null(result);
    }


    [Fact]
    public async Task GetProfileAsync_ShouldReturnCorrectUserData()
    {
        // Arrange
        var userRepository = new Mock<IUserRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var jwtTokenService = CreateJwtTokenServiceMock();

        var user = new User
        {
            Id = 5,
            FirstName = "Rahul",
            LastName = "Sharma",
            Email = "rahul@example.com",
            PhoneNumber = "9999999999",
            Country = "India",
            IsActive = true
        };

        userRepository
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(user);

        var service = new AuthService(
            userRepository.Object,
            unitOfWork.Object,
            jwtTokenService.Object);

        // Act
        var result = await service.GetProfileAsync(5);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FirstName, result.FirstName);
        Assert.Equal(user.LastName, result.LastName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.PhoneNumber, result.PhoneNumber);
        Assert.Equal(user.Country, result.Country);
    }


    // =========================================================
    // Helper
    // =========================================================

    private static Mock<IJwtTokenService> CreateJwtTokenServiceMock()
    {
        var mock = new Mock<IJwtTokenService>();

        mock
            .Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns("test-jwt-token");

        return mock;
    }
}