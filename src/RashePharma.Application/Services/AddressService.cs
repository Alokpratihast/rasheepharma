using RashePharma.Application.DTOs.Addresses;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddressService(
        IAddressRepository addressRepository,
        IUnitOfWork unitOfWork)
    {
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AddressListDto>> GetByUserIdAsync(int userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);

        return addresses.Select(a => new AddressListDto
        {
            Id = a.Id,
            AddressLine1 = a.AddressLine1,
            AddressLine2 = a.AddressLine2,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            AddressType = a.AddressType,
            IsDefault = a.IsDefault
        }).ToList();
    }

    public async Task<AddressDetailsDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);

        if (address == null || address.UserId != userId)
            return null;

        return MapToDetailsDto(address);
    }

    public async Task<AddressDetailsDto> CreateAsync(
        int userId,
        AddressCreateDto dto)
    {
        var address = new Address
        {
            UserId = userId,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            AddressType = dto.AddressType,
            IsDefault = dto.IsDefault
        };

        await _addressRepository.AddAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return MapToDetailsDto(address);
    }

    public async Task<AddressDetailsDto?> UpdateAsync(
        int id,
        int userId,
        AddressUpdateDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(id);

        if (address == null || address.UserId != userId)
            return null;

        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;
        address.AddressType = dto.AddressType;
        address.IsDefault = dto.IsDefault;
        address.UpdatedAt = DateTime.UtcNow;

        await _addressRepository.UpdateAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return MapToDetailsDto(address);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var address = await _addressRepository.GetByIdAsync(id);

        if (address == null || address.UserId != userId)
            return false;

        await _addressRepository.DeleteAsync(address);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static AddressDetailsDto MapToDetailsDto(
        Address address)
    {
        return new AddressDetailsDto
        {
            Id = address.Id,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            AddressType = address.AddressType,
            IsDefault = address.IsDefault
        };
    }
}