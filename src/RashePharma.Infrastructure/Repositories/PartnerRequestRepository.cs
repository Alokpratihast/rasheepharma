using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class PartnerRequestRepository : IPartnerRequestRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PartnerRequest>> GetAllAsync()
    {
        return await _context.PartnerRequests
            .Include(pr => pr.User)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task<PartnerRequest?> GetByIdAsync(int id)
    {
        return await _context.PartnerRequests
            .Include(pr => pr.User)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<List<PartnerRequest>> GetByUserIdAsync(int userId)
    {
        return await _context.PartnerRequests
            .Where(pr => pr.UserId == userId)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(PartnerRequest request)
    {
        await _context.PartnerRequests.AddAsync(request);
    }

    public async Task UpdateAsync(PartnerRequest request)
    {
        _context.PartnerRequests.Update(request);
        await Task.CompletedTask;
    }
}