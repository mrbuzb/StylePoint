using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class DiscountRepository : IDiscountRepository
{
    private readonly AppDbContext _context;
    public DiscountRepository(AppDbContext context) => _context = context;


    public async Task<decimal> ApplyDiscountAsync(long userId, string code)
    {
        var promo = await _context.Discounts
            .Include(p => p.RedeemedUsers)
            .FirstOrDefaultAsync(p => p.Code == code);

        if (promo == null)
            throw new Exception("Promo code not found.");

        if (promo.ExpiryDate < DateTime.UtcNow)
        {
            promo.IsActive = false;
            _context.Discounts.Update(promo);
            await _context.SaveChangesAsync();
            throw new Exception("Promo code has expired.");
        }

        if (promo.UsageLimit <= promo.RedeemedUsers.Count)
        {
            promo.IsActive = false;
            _context.Discounts.Update(promo);
            await _context.SaveChangesAsync();
            throw new Exception("Promo code usage limit reached.");
        }
            

        bool alreadyUsed = promo.RedeemedUsers.Any(x => x.UserId == userId);

        if (alreadyUsed)
            throw new Exception("You have already used this promo code.");

        var usage = new UserDiscount
        {
            UserId = userId,
            DiscountId = promo.Id,
            UsedAt = DateTime.UtcNow
        };

        await _context.UserDiscounts.AddAsync(usage);

        await _context.SaveChangesAsync();

        return promo.Percentage;
    }




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
        return await _context.Discounts.Include(x=>x.RedeemedUsers).FirstOrDefaultAsync(x=>x.Code == code);
    }
}
