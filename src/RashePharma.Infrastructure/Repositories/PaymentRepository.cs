using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByStripeSessionIdAsync(
        string stripeSessionId)
    {
        return await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(
                p => p.StripeSessionId == stripeSessionId);
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);

        await Task.CompletedTask;
    }
}