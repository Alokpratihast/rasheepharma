using RashePharma.Application.DTOs.Enquiries;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class EnquiryService : IEnquiryService
{
    private readonly IEnquiryRepository _enquiryRepository;
    private readonly IProductVariantRepository _variantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EnquiryService(
        IEnquiryRepository enquiryRepository,
        IProductVariantRepository variantRepository,
        IUnitOfWork unitOfWork)
    {
        _enquiryRepository = enquiryRepository;
        _variantRepository = variantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EnquiryListDto>> GetAllAsync()
    {
        var enquiries = await _enquiryRepository.GetAllAsync();

        return enquiries
            .Select(MapToListDto)
            .ToList();
    }

    public async Task<List<EnquiryListDto>> GetByUserIdAsync(int userId)
    {
        var enquiries =
            await _enquiryRepository.GetByUserIdAsync(userId);

        return enquiries
            .Select(MapToListDto)
            .ToList();
    }

    public async Task<EnquiryDetailsDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var enquiry =
            await _enquiryRepository.GetByIdAsync(id);

        if (enquiry == null)
            return null;

        // Customer can only view their own enquiry.
        if (enquiry.UserId != userId)
            return null;

        return MapToDetailsDto(enquiry);
    }

    public async Task<EnquiryDetailsDto?> GetByIdForAdminAsync(int id)
    {
        var enquiry =
            await _enquiryRepository.GetByIdAsync(id);

        if (enquiry == null)
            return null;

        return MapToDetailsDto(enquiry);
    }

    public async Task<EnquiryDetailsDto?> GetByEnquiryNumberAsync(
        string enquiryNumber,
        int userId)
    {
        var enquiry =
            await _enquiryRepository
                .GetByEnquiryNumberAsync(enquiryNumber);

        if (enquiry == null)
            return null;

        // Customer can only view their own enquiry.
        if (enquiry.UserId != userId)
            return null;

        return MapToDetailsDto(enquiry);
    }

    public async Task<EnquiryDetailsDto?>
        GetByEnquiryNumberForAdminAsync(
            string enquiryNumber)
    {
        var enquiry =
            await _enquiryRepository
                .GetByEnquiryNumberAsync(enquiryNumber);

        if (enquiry == null)
            return null;

        return MapToDetailsDto(enquiry);
    }

    public async Task<EnquiryDetailsDto> CreateAsync(
        CreateEnquiryDto dto,
        int? userId)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var enquiry = new Enquiry
        {
            UserId = userId,
            EnquiryNumber = GenerateEnquiryNumber(),
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,
            BusinessType = dto.BusinessType,
            Message = dto.Message,
            Status = "Pending"
        };

        // Enquiry items are optional.
        // An enquiry can be created without selecting products.
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Enquiry item quantity must be greater than zero.");
            }

            var variant =
                await _variantRepository
                    .GetByIdAsync(item.ProductVariantId);

            if (variant == null)
            {
                throw new InvalidOperationException(
                    $"Product variant with id {item.ProductVariantId} was not found.");
            }

            if (!variant.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product variant with id {item.ProductVariantId} is inactive.");
            }

            if (variant.Product == null)
            {
                throw new InvalidOperationException(
                    $"Product for variant {item.ProductVariantId} could not be loaded.");
            }

            if (!variant.Product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product for variant {item.ProductVariantId} is inactive.");
            }

            enquiry.Items.Add(new EnquiryItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                Message = item.Message
            });
        }

        await _enquiryRepository.AddAsync(enquiry);

        await _unitOfWork.SaveChangesAsync();

        var createdEnquiry =
            await _enquiryRepository.GetByIdAsync(enquiry.Id);

        if (createdEnquiry == null)
        {
            throw new InvalidOperationException(
                "Enquiry could not be loaded after creation.");
        }

        return MapToDetailsDto(createdEnquiry);
    }

    public async Task<bool> UpdateStatusAsync(
        int id,
        UpdateEnquiryStatusDto dto)
    {
        var enquiry =
            await _enquiryRepository.GetByIdAsync(id);

        if (enquiry == null)
            return false;

        if (string.IsNullOrWhiteSpace(dto.Status))
        {
            throw new InvalidOperationException(
                "Enquiry status is required.");
        }

        var allowedStatuses = new[]
        {
            "Pending",
            "Contacted",
            "Quoted",
            "Closed",
            "Rejected"
        };

        if (!allowedStatuses.Contains(
                dto.Status,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Invalid enquiry status '{dto.Status}'.");
        }

        enquiry.Status = dto.Status;
        enquiry.UpdatedAt = DateTime.UtcNow;

        await _enquiryRepository.UpdateAsync(enquiry);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static string GenerateEnquiryNumber()
    {
        return $"ENQ-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    private static EnquiryListDto MapToListDto(
        Enquiry enquiry)
    {
        return new EnquiryListDto
        {
            Id = enquiry.Id,
            EnquiryNumber = enquiry.EnquiryNumber,
            CustomerName = enquiry.CustomerName,
            Email = enquiry.Email,
            Country = enquiry.Country,
            BusinessType = enquiry.BusinessType,
            Status = enquiry.Status,
            CreatedAt = enquiry.CreatedAt
        };
    }

    private static EnquiryDetailsDto MapToDetailsDto(
        Enquiry enquiry)
    {
        return new EnquiryDetailsDto
        {
            Id = enquiry.Id,
            EnquiryNumber = enquiry.EnquiryNumber,
            CustomerName = enquiry.CustomerName,
            Email = enquiry.Email,
            PhoneNumber = enquiry.PhoneNumber,
            Country = enquiry.Country,
            BusinessType = enquiry.BusinessType,
            Message = enquiry.Message,
            Status = enquiry.Status,
            CreatedAt = enquiry.CreatedAt,

            Items = enquiry.Items
                .Select(item => new EnquiryItemDto
                {
                    Id = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductVariant.Product.Name,
                    Strength = item.ProductVariant.Strength,
                    PackSize = item.ProductVariant.PackSize,
                    Quantity = item.Quantity,
                    Message = item.Message
                })
                .ToList()
        };
    }
}