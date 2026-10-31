
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailDeliveryEventRepository
    : IEmailDeliveryEventRepository
{
    private readonly ApplicationDbContext _context;

    public EmailDeliveryEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailDeliveryEvent?> GetByIdAsync(Guid id)
    {
        return await _context.Set<EmailDeliveryEvent>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(EmailDeliveryEvent deliveryEvent)
    {
        await _context.Set<EmailDeliveryEvent>()
            .AddAsync(deliveryEvent);
    }
}
