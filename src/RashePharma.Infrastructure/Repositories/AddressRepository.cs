using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _context;

    public AddressRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Address>> GetByUserIdAsync(int userId)
    {
        return await _context.Addresses
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<Address?> GetByIdAsync(int id)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
    }

    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Address address)
    {
        _context.Addresses.Remove(address);

        await Task.CompletedTask;
    }
}