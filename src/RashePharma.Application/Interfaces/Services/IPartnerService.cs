using RashePharma.Application.DTOs.Partners;

namespace RashePharma.Application.Interfaces.Services;

public interface IPartnerService
{
    Task<List<PartnerRequestListDto>> GetAllRequestsAsync();
    Task<PartnerRequestDetailsDto?> GetRequestByIdAsync(int id);
    Task<List<PartnerRequestListDto>> GetMyRequestsAsync(int userId);

    Task<PartnerRequestDetailsDto> CreateRequestAsync(
        PartnerRequestCreateDto dto,
        int? userId);

    Task<bool> UpdateRequestStatusAsync(
        int id,
        UpdatePartnerRequestStatusDto dto);

    Task<List<PartnerDto>> GetAllPartnersAsync();
    Task<PartnerDto?> GetPartnerByIdAsync(int id);
    Task<PartnerDto?> GetMyPartnerAsync(int userId);
}