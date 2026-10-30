
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailQuotaReservationRepository
    : IEmailQuotaReservationRepository
{
    private readonly ApplicationDbContext _context;

    public EmailQuotaReservationRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailQuotaReservation?> GetByIdAsync(Guid id)
    {
        return await _context.Set<EmailQuotaReservation>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(EmailQuotaReservation reservation)
    {
        await _context.Set<EmailQuotaReservation>()
            .AddAsync(reservation);
    }

    public Task UpdateAsync(EmailQuotaReservation reservation)
    {
        _context.Set<EmailQuotaReservation>().Update(reservation);
        return Task.CompletedTask;
    }
}
