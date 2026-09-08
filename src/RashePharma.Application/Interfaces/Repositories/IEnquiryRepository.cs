using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEnquiryRepository
{
    Task<List<Enquiry>> GetAllAsync();

    Task<List<Enquiry>> GetByUserIdAsync(int userId);

    Task<Enquiry?> GetByIdAsync(int id);

    Task<Enquiry?> GetByEnquiryNumberAsync(string enquiryNumber);

    Task AddAsync(Enquiry enquiry);

    Task UpdateAsync(Enquiry enquiry);
}