using RashePharma.Application.DTOs.Addresses;

namespace RashePharma.Application.Interfaces.Services;

public interface IAddressService
{
    Task<List<AddressListDto>> GetByUserIdAsync(int userId);
    Task<AddressDetailsDto?> GetByIdAsync(int id, int userId);
    Task<AddressDetailsDto> CreateAsync(int userId, AddressCreateDto dto);
    Task<AddressDetailsDto?> UpdateAsync(int id, int userId, AddressUpdateDto dto);
    Task<bool> DeleteAsync(int id, int userId);
}