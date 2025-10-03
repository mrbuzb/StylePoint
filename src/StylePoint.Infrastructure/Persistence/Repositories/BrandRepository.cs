using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class BrandRepository : IBrandRepository
{
    private readonly AppDbContext _context;
    public BrandRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<Brand>> GetAllAsync()
        => await _context.Brands.ToListAsync();

    public async Task<Brand?> GetByIdAsync(long id)
        => await _context.Brands.FindAsync(id);

    public async Task AddAsync(Brand brand)
    {
        await _context.Brands.AddAsync(brand);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Brand brand)
    {
        _context.Brands.Update(brand);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Brands.FindAsync(id);
        if (entity != null)
        {
            _context.Brands.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
