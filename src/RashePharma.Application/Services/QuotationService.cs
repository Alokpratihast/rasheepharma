using RashePharma.Application.DTOs.Quotations;

using RashePharma.Application.Interfaces;

using RashePharma.Application.Interfaces.Repositories;

using RashePharma.Application.Interfaces.Services;

using RashePharma.Domain.Entities;



namespace RashePharma.Application.Services;



public class QuotationService : IQuotationService

{

    private readonly IQuotationRepository _quotationRepository;

    private readonly IEnquiryRepository _enquiryRepository;

    private readonly IProductVariantRepository _variantRepository;

    private readonly IUnitOfWork _unitOfWork;



    private readonly IEmailNotificationRepository _emailNotificationRepository;



    private static readonly HashSet<string> AllowedStatuses =

        new(StringComparer.OrdinalIgnoreCase)

        {

            "Draft",

            "Sent",

            "Accepted",

            "Rejected",

            "Expired"

        };



    private static readonly HashSet<string> AllowedCurrencies =

        new(StringComparer.OrdinalIgnoreCase)

        {

            "USD",

            "EUR",

            "GBP",

            "INR"

        };



    public QuotationService(

        IQuotationRepository quotationRepository,

        IEnquiryRepository enquiryRepository,

        IProductVariantRepository variantRepository,

        IEmailNotificationRepository emailNotificationRepository,

        IUnitOfWork unitOfWork)

    {

        _quotationRepository = quotationRepository;

        _enquiryRepository = enquiryRepository;

        _variantRepository = variantRepository;

                _emailNotificationRepository = emailNotificationRepository;
_unitOfWork = unitOfWork;

    }



    public async Task<List<QuotationListDto>> GetAllAsync()

    {

        var quotations =

            await _quotationRepository.GetAllAsync();



        return quotations

            .Select(MapToListDto)

            .ToList();

    }



    public async Task<List<QuotationListDto>> GetByUserIdAsync(

        int userId)

    {

        var quotations =

            await _quotationRepository.GetByUserIdAsync(userId);



        return quotations

            .Select(MapToListDto)

            .ToList();

    }



    public async Task<List<QuotationListDto>> GetByEnquiryIdAsync(

        int enquiryId,

        int userId)

    {

        var quotations =

            await _quotationRepository

                .GetByEnquiryIdAsync(enquiryId);



        return quotations

            .Where(q => q.UserId == userId)

            .Select(MapToListDto)

            .ToList();

    }



    public async Task<List<QuotationListDto>> GetByEnquiryIdForAdminAsync(

        int enquiryId)

    {

        var quotations =

            await _quotationRepository

                .GetByEnquiryIdAsync(enquiryId);



        return quotations

            .Select(MapToListDto)

            .ToList();

    }



    public async Task<QuotationDetailsDto?> GetByIdAsync(

        int id,

        int userId)

    {

        var quotation =

            await _quotationRepository.GetByIdAsync(id);



        if (quotation == null)

            return null;



        if (quotation.UserId != userId)

            return null;



        return MapToDetailsDto(quotation);

    }



    public async Task<QuotationDetailsDto?> GetByIdForAdminAsync(

        int id)

    {

        var quotation =

            await _quotationRepository.GetByIdAsync(id);



        if (quotation == null)

            return null;



        return MapToDetailsDto(quotation);

    }



    public async Task<QuotationDetailsDto?> GetByQuoteNumberAsync(

        string quoteNumber,

        int userId)

    {

        var quotation =

            await _quotationRepository

                .GetByQuoteNumberAsync(quoteNumber);



        if (quotation == null)

            return null;



        if (quotation.UserId != userId)

            return null;



        return MapToDetailsDto(quotation);

    }



    public async Task<QuotationDetailsDto?> GetByQuoteNumberForAdminAsync(

        string quoteNumber)

    {

        var quotation =

            await _quotationRepository

                .GetByQuoteNumberAsync(quoteNumber);



        if (quotation == null)

            return null;



        return MapToDetailsDto(quotation);

    }



    public async Task<QuotationDetailsDto> CreateAsync(

        CreateQuotationDto dto,

        int userId)

    {

        if (dto == null)

        {

            throw new ArgumentNullException(nameof(dto));

        }



        if (dto.Items == null || dto.Items.Count == 0)

        {

            throw new InvalidOperationException(

                "Quotation must contain at least one item.");

        }



        if (dto.ValidUntil <= DateTime.UtcNow)

        {

            throw new InvalidOperationException(

                "Quotation validity date must be in the future.");

        }



        if (string.IsNullOrWhiteSpace(dto.Currency))

        {

            throw new InvalidOperationException(

                "Currency is required.");

        }



        if (dto.Notes?.Length > 2000)

        {

            throw new InvalidOperationException(

                "Quotation notes cannot exceed 2000 characters.");

        }



        var currency =

            dto.Currency.Trim().ToUpperInvariant();



        if (!AllowedCurrencies.Contains(currency))

        {

            throw new InvalidOperationException(

                $"Unsupported currency: {dto.Currency}");

        }



        // Load the enquiry.

        var enquiry =

            await _enquiryRepository.GetByIdAsync(dto.EnquiryId);



        if (enquiry == null)

        {

            throw new InvalidOperationException(

                "Enquiry not found.");

        }



        // Customer email is required because the quotation

        // will be sent to the email stored on the enquiry.

        if (string.IsNullOrWhiteSpace(enquiry.Email))

        {

            throw new InvalidOperationException(

                "Customer email is required to send the quotation.");

        }



        var quotation = new Quotation

        {

            EnquiryId = dto.EnquiryId,

            UserId = enquiry.UserId,

            QuoteNumber = GenerateQuoteNumber(),

            Currency = currency,

            Status = "Draft",

            ValidUntil = dto.ValidUntil,

            Notes = dto.Notes?.Trim()

        };



        foreach (var item in dto.Items)

        {

            if (item.ProductVariantId <= 0)

            {

                throw new InvalidOperationException(

                    "Product variant is required.");

            }



            if (item.Quantity <= 0)

            {

                throw new InvalidOperationException(

                    "Quotation item quantity must be greater than zero.");

            }



            if (item.Quantity > 1_000_000)

            {

                throw new InvalidOperationException(

                    "Quotation item quantity cannot exceed 1,000,000.");

            }



            // Load the variant from the database.

            var variant =

                await _variantRepository

                    .GetByIdAsync(item.ProductVariantId);



            if (variant == null)

            {

                throw new InvalidOperationException(

                    $"Product variant {item.ProductVariantId} not found.");

            }



            if (!variant.IsActive)

            {

                throw new InvalidOperationException(

                    $"Product variant {item.ProductVariantId} is inactive.");

            }



            if (variant.Product == null ||

                !variant.Product.IsActive)

            {

                throw new InvalidOperationException(

                    $"Product for variant {item.ProductVariantId} is inactive.");

            }



            // Unit price is supplied by the authorized admin

            // creating the quotation.

            if (item.UnitPrice <= 0)

            {

                throw new InvalidOperationException(

                    "Quotation item unit price must be greater than zero.");

            }



            if (item.UnitPrice > 1_000_000_000m)

            {

                throw new InvalidOperationException(

                    "Quotation item unit price is too large.");

            }



            var unitPrice = decimal.Round(

                item.UnitPrice,

                2,

                MidpointRounding.AwayFromZero);



            var totalPrice =

                unitPrice * item.Quantity;



            var quotationItem = new QuotationItem

            {

                ProductVariantId = item.ProductVariantId,

                ProductVariant = variant,

                Quantity = item.Quantity,

                UnitPrice = unitPrice,

                TotalPrice = totalPrice

            };



            quotation.Items.Add(quotationItem);



            quotation.TotalAmount += totalPrice;

        }



        var transaction = await _unitOfWork.BeginTransactionAsync();

try
{
    await _quotationRepository.AddAsync(quotation);

    await _unitOfWork.SaveChangesAsync();

    var notification = new EmailNotification
    {
        QuotationId = quotation.Id,
        Type = "Quotation",
        Status = "Pending",
        AttemptCount = 0,
        CreatedAt = DateTime.UtcNow
    };

    await _emailNotificationRepository.AddAsync(notification);

    await _unitOfWork.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
finally
{
    await transaction.DisposeAsync();
}

var createdQuotation =

            await _quotationRepository

                .GetByIdAsync(quotation.Id);



        if (createdQuotation == null)

        {

            throw new InvalidOperationException(

                "Quotation could not be loaded after creation.");

        }



        return MapToDetailsDto(createdQuotation);

    }



    public async Task<bool> UpdateStatusAsync(

        int id,

        UpdateQuotationStatusDto dto)

    {

        if (dto == null)

        {

            throw new ArgumentNullException(nameof(dto));

        }



        if (string.IsNullOrWhiteSpace(dto.Status))

        {

            throw new InvalidOperationException(

                "Quotation status is required.");

        }



        var status = dto.Status.Trim();



        if (!AllowedStatuses.Contains(status))

        {

            throw new InvalidOperationException(

                $"Invalid quotation status: {dto.Status}");

        }



        var quotation =

            await _quotationRepository.GetByIdAsync(id);



        if (quotation == null)

            return false;



        quotation.Status = status;

        quotation.UpdatedAt = DateTime.UtcNow;



        await _quotationRepository.UpdateAsync(quotation);



        await _unitOfWork.SaveChangesAsync();



        return true;

    }



    private static string GenerateQuoteNumber()

    {

        return $"QUO-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    }



    private static QuotationListDto MapToListDto(

        Quotation quotation)

    {

        return new QuotationListDto

        {

            Id = quotation.Id,

            QuoteNumber = quotation.QuoteNumber,

            EnquiryId = quotation.EnquiryId,

            TotalAmount = quotation.TotalAmount,

            Currency = quotation.Currency,

            Status = quotation.Status,

            ValidUntil = quotation.ValidUntil,

            CreatedAt = quotation.CreatedAt

        };

    }



    private static QuotationDetailsDto MapToDetailsDto(

        Quotation quotation)

    {

        return new QuotationDetailsDto

        {

            Id = quotation.Id,

            QuoteNumber = quotation.QuoteNumber,

            EnquiryId = quotation.EnquiryId,

            TotalAmount = quotation.TotalAmount,

            Currency = quotation.Currency,

            Status = quotation.Status,

            ValidUntil = quotation.ValidUntil,

            Notes = quotation.Notes,

            CreatedAt = quotation.CreatedAt,

            UpdatedAt = quotation.UpdatedAt,



            Items = quotation.Items

                .Select(item => new QuotationItemDto

                {

                    Id = item.Id,

                    ProductVariantId = item.ProductVariantId,

                    ProductName = item.ProductVariant.Product.Name,

                    Strength = item.ProductVariant.Strength,

                    PackSize = item.ProductVariant.PackSize,

                    Quantity = item.Quantity,

                    UnitPrice = item.UnitPrice,

                    TotalPrice = item.TotalPrice

                })

                .ToList()

        };

    }

}
