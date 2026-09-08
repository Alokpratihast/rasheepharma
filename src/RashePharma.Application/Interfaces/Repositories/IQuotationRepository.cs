using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IQuotationRepository
{
    Task<List<Quotation>> GetAllAsync();

    Task<List<Quotation>> GetByUserIdAsync(int userId);

    Task<List<Quotation>> GetByEnquiryIdAsync(int enquiryId);

    Task<Quotation?> GetByIdAsync(int id);

    Task<Quotation?> GetByQuoteNumberAsync(string quoteNumber);

    Task AddAsync(Quotation quotation);

    Task UpdateAsync(Quotation quotation);
}