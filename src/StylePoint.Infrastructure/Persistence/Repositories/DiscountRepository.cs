using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly AppDbContext _context;
    public DiscountRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<Discount>> GetAllAsync()
        => await _context.Discounts.ToListAsync();

    public async Task<Discount?> GetByIdAsync(long id)
        => await _context.Discounts.FindAsync(id);

    public async Task AddAsync(Discount discount)
    {
        await _context.Discounts.AddAsync(discount);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Discount discount)
    {
        _context.Discounts.Update(discount);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Discounts.FindAsync(id);
        if (entity != null)
        {
            _context.Discounts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Discount?> GetByCodeAsync(string code)
    {
        return await _context.Discounts.FindAsync(code);
    }
}
