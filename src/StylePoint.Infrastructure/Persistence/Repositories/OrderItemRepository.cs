using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly AppDbContext _context;
    public OrderItemRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<OrderItem>> GetAllAsync()
        => await _context.OrderItems.Include(oi => oi.ProductVariant)
                                    .ToListAsync();

    public async Task<OrderItem?> GetByIdAsync(long id)
        => await _context.OrderItems.Include(oi => oi.ProductVariant)
                                    .FirstOrDefaultAsync(oi => oi.Id == id);

    public async Task AddAsync(OrderItem item)
    {
        await _context.OrderItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(ICollection<OrderItem> items)
    {
        await _context.OrderItems.AddRangeAsync(items);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(OrderItem item)
    {
        _context.OrderItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.OrderItems.FindAsync(id);
        if (entity != null)
        {
            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
