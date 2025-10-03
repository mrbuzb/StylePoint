using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class CartItemRepository : ICartItemRepository
{
    private readonly AppDbContext _context;
    public CartItemRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<CartItem>> GetAllAsync()
        => await _context.CartItems.Include(c => c.ProductVariant)
                                   .ToListAsync();

    public async Task<CartItem?> GetByIdAsync(long id)
        => await _context.CartItems.Include(c => c.ProductVariant)
                                   .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ICollection<CartItem>> GetUserCartAsync(long userId) =>
        await _context.CartItems.Include(c => c.ProductVariant)
                           .Where(c => c.UserId == userId)
                           .ToListAsync();

    public async Task ClearUserCartAsync(long userId)
    {
        var items = _context.CartItems.Where(c => c.UserId == userId);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public async Task AddAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CartItem item)
    {
        _context.CartItems.Update(item);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.CartItems.FindAsync(id);
        if (entity != null)
        {
            _context.CartItems.Remove(entity);

            await _context.SaveChangesAsync();
        }
    }
}
