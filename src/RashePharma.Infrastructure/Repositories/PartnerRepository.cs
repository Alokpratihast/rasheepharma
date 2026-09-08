using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class PartnerRepository : IPartnerRepository
{
    private readonly ApplicationDbContext _context;

    public PartnerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Partner>> GetAllAsync()
    {
        return await _context.Partners
            .Include(p => p.User)
            .Include(p => p.PartnerRequest)
            .OrderByDescending(p => p.JoinedAt)
            .ToListAsync();
    }

    public async Task<Partner?> GetByIdAsync(int id)
    {
        return await _context.Partners
            .Include(p => p.User)
            .Include(p => p.PartnerRequest)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Partner?> GetByUserIdAsync(int userId)
    {
        return await _context.Partners
            .Include(p => p.PartnerRequest)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task AddAsync(Partner partner)
    {
        await _context.Partners.AddAsync(partner);
    }

    public async Task UpdateAsync(Partner partner)
    {
        _context.Partners.Update(partner);
        await Task.CompletedTask;
    }
}