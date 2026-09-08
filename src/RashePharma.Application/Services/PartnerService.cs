using RashePharma.Application.DTOs.Partners;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class PartnerService : IPartnerService
{
    private readonly IPartnerRequestRepository _requestRepository;
    private readonly IPartnerRepository _partnerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartnerService(
        IPartnerRequestRepository requestRepository,
        IPartnerRepository partnerRepository,
        IUnitOfWork unitOfWork)
    {
        _requestRepository = requestRepository;
        _partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PartnerRequestListDto>> GetAllRequestsAsync()
    {
        var requests = await _requestRepository.GetAllAsync();

        return requests.Select(r => new PartnerRequestListDto
        {
            Id = r.Id,
            CompanyName = r.CompanyName,
            ContactPerson = r.ContactPerson,
            Email = r.Email,
            Country = r.Country,
            BusinessType = r.BusinessType,
            ExpectedVolume = r.ExpectedVolume,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<PartnerRequestDetailsDto?> GetRequestByIdAsync(
        int id)
    {
        var request = await _requestRepository.GetByIdAsync(id);

        return request == null
            ? null
            : MapRequestToDetailsDto(request);
    }

    public async Task<List<PartnerRequestListDto>> GetMyRequestsAsync(
        int userId)
    {
        var requests = await _requestRepository
            .GetByUserIdAsync(userId);

        return requests.Select(r => new PartnerRequestListDto
        {
            Id = r.Id,
            CompanyName = r.CompanyName,
            ContactPerson = r.ContactPerson,
            Email = r.Email,
            Country = r.Country,
            BusinessType = r.BusinessType,
            ExpectedVolume = r.ExpectedVolume,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<PartnerRequestDetailsDto> CreateRequestAsync(
        PartnerRequestCreateDto dto,
        int? userId)
    {
        var request = new PartnerRequest
        {
            UserId = userId,
            CompanyName = dto.CompanyName,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,
            BusinessType = dto.BusinessType,
            ExpectedVolume = dto.ExpectedVolume,
            Message = dto.Message,
            Status = "Pending"
        };

        await _requestRepository.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return MapRequestToDetailsDto(request);
    }

    public async Task<bool> UpdateRequestStatusAsync(
        int id,
        UpdatePartnerRequestStatusDto dto)
    {
        var request = await _requestRepository.GetByIdAsync(id);

        if (request == null)
            return false;

        request.Status = dto.Status;
        request.UpdatedAt = DateTime.UtcNow;

        await _requestRepository.UpdateAsync(request);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<PartnerDto>> GetAllPartnersAsync()
    {
        var partners = await _partnerRepository.GetAllAsync();

        return partners.Select(MapPartnerToDto).ToList();
    }

    public async Task<PartnerDto?> GetPartnerByIdAsync(int id)
    {
        var partner = await _partnerRepository.GetByIdAsync(id);

        return partner == null
            ? null
            : MapPartnerToDto(partner);
    }

    public async Task<PartnerDto?> GetMyPartnerAsync(int userId)
    {
        var partner = await _partnerRepository
            .GetByUserIdAsync(userId);

        return partner == null
            ? null
            : MapPartnerToDto(partner);
    }

    private static PartnerRequestDetailsDto MapRequestToDetailsDto(
        PartnerRequest request)
    {
        return new PartnerRequestDetailsDto
        {
            Id = request.Id,
            CompanyName = request.CompanyName,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Country = request.Country,
            BusinessType = request.BusinessType,
            ExpectedVolume = request.ExpectedVolume,
            Message = request.Message,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }

    private static PartnerDto MapPartnerToDto(Partner partner)
    {
        return new PartnerDto
        {
            Id = partner.Id,
            CompanyName = partner.CompanyName,
            ContactPerson = partner.ContactPerson,
            Email = partner.Email,
            PhoneNumber = partner.PhoneNumber,
            Country = partner.Country,
            BusinessType = partner.BusinessType,
            RegistrationNumber = partner.RegistrationNumber,
            TaxIdentificationNumber = partner.TaxIdentificationNumber,
            Status = partner.Status,
            JoinedAt = partner.JoinedAt
        };
    }
}