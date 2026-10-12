using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class StripeWebhookEventRepository : IStripeWebhookEventRepository
{
    private readonly ApplicationDbContext _context;

    public StripeWebhookEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StripeWebhookEvent?> GetByStripeEventIdAsync(
        string stripeEventId)
    {
        return await _context.StripeWebhookEvents
            .FirstOrDefaultAsync(
                e => e.StripeEventId == stripeEventId);
    }

    public async Task AddAsync(StripeWebhookEvent webhookEvent)
    {
        await _context.StripeWebhookEvents.AddAsync(webhookEvent);
    }

    public async Task UpdateAsync(StripeWebhookEvent webhookEvent)
    {
        _context.StripeWebhookEvents.Update(webhookEvent);
        await Task.CompletedTask;
    }
}