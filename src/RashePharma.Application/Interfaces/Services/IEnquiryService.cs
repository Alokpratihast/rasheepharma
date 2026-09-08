using RashePharma.Application.DTOs.Enquiries;

namespace RashePharma.Application.Interfaces.Services;

public interface IEnquiryService
{
    Task<List<EnquiryListDto>> GetAllAsync();
    Task<List<EnquiryListDto>> GetByUserIdAsync(int userId);

    Task<EnquiryDetailsDto?> GetByIdAsync(int id, int userId);
    Task<EnquiryDetailsDto?> GetByIdForAdminAsync(int id);

    Task<EnquiryDetailsDto?> GetByEnquiryNumberAsync(
        string enquiryNumber,
        int userId);

    Task<EnquiryDetailsDto?> GetByEnquiryNumberForAdminAsync(
        string enquiryNumber);

    Task<EnquiryDetailsDto> CreateAsync(
        CreateEnquiryDto dto,
        int? userId);

    Task<bool> UpdateStatusAsync(
        int id,
        UpdateEnquiryStatusDto dto);
}
