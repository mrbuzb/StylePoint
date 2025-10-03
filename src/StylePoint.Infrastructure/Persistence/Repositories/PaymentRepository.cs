using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;
    public PaymentRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<Payment>> GetAllAsync()
        => await _context.Payments.ToListAsync();

    public async Task<Payment?> GetByIdAsync(long id)
        => await _context.Payments.FindAsync(id);

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Payments.FindAsync(id);
        if (entity != null)
        {
            _context.Payments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
