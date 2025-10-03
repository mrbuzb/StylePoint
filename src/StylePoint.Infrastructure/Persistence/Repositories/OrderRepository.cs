using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<Order>> GetAllAsync()
        => await _context.Orders.Include(o => o.User)
                                .Include(o => o.OrderItems)
                                .ToListAsync();

    public async Task<Order?> GetByIdAsync(long id)
        => await _context.Orders.Include(o => o.User)
                                .Include(o => o.OrderItems)
                                .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<ICollection<Order>> GetByUserIdAsync(long userId) =>
        await _context.Orders.Include(o => o.OrderItems)
                        .Where(o => o.UserId == userId)
                        .OrderByDescending(o => o.CreatedAt)
                        .ToListAsync();
    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity != null)
        {
            _context.Orders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
