using Microsoft.AspNetCore.Identity;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = new PasswordHasher<User>();
    }

   public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
{
    var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

    if (existingUser != null)
        throw new InvalidOperationException(
            "A user with this email already exists.");

    var user = new User
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        PhoneNumber = dto.PhoneNumber,
        Country = dto.Country,
        RoleId = 2,
        IsActive = true
    };

    user.PasswordHash = _passwordHasher.HashPassword(
        user,
        dto.Password);

    await _userRepository.AddAsync(user);
    await _unitOfWork.SaveChangesAsync();

    // Reload user with Role navigation property
    var savedUser = await _userRepository.GetByEmailAsync(dto.Email);

    if (savedUser == null)
        throw new InvalidOperationException(
            "User could not be loaded after registration.");

    var token = _jwtTokenService.GenerateToken(savedUser);

    return new AuthResponseDto
    {
        UserId = savedUser.Id,
        FirstName = savedUser.FirstName,
        LastName = savedUser.LastName,
        Email = savedUser.Email,
        Token = token
    };
}

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
            return null;

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return null;

        var token = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Country = user.Country
        };
    }
}