using RashePharma.Application.DTOs.Quotations;

namespace RashePharma.Application.Interfaces.Services;

public interface IQuotationService
{
    Task<List<QuotationListDto>> GetAllAsync();

    Task<List<QuotationListDto>> GetByUserIdAsync(
        int userId);

    Task<List<QuotationListDto>> GetByEnquiryIdAsync(
        int enquiryId,
        int userId);

    Task<List<QuotationListDto>> GetByEnquiryIdForAdminAsync(
        int enquiryId);

    Task<QuotationDetailsDto?> GetByIdAsync(
        int id,
        int userId);

    Task<QuotationDetailsDto?> GetByIdForAdminAsync(
        int id);

    Task<QuotationDetailsDto?> GetByQuoteNumberAsync(
        string quoteNumber,
        int userId);

    Task<QuotationDetailsDto?> GetByQuoteNumberForAdminAsync(
        string quoteNumber);

    Task<QuotationDetailsDto> CreateAsync(
        CreateQuotationDto dto,
        int userId);

    Task<bool> UpdateStatusAsync(
        int id,
        UpdateQuotationStatusDto dto);
}