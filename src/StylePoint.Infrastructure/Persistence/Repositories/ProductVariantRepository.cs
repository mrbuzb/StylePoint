using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly AppDbContext _context;
    public ProductVariantRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<ProductVariant>> GetAllAsync()
        => await _context.ProductVariants.ToListAsync();

    public async Task<ProductVariant?> GetByIdAsync(long id)
        => await _context.ProductVariants.FindAsync(id);

    public async Task AddAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.ProductVariants.FindAsync(id);
        if (entity != null)
        {
            _context.ProductVariants.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
