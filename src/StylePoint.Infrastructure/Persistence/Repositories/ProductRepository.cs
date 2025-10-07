using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(long id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Variants)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<ICollection<Product>> SearchAsync(string keyword)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p =>
                p.Name.Contains(keyword) ||
                p.Description.Contains(keyword))
            .ToListAsync();
    }

    public async Task<ICollection<Product>> GetByCategoryAsync(long categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<ICollection<Product>> GetByBrandAsync(long brandId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.BrandId == brandId)
            .ToListAsync();
    }

    public async Task<ICollection<Product>> GetByTagAsync(long tagId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
            .Where(p => p.ProductTags.Any(pt => pt.TagId == tagId))
            .ToListAsync();
    }

    public async Task<ICollection<Product>> GetBestSellersAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Price)
            .Take(10)
            .ToListAsync();
    }

    public async Task<ICollection<Product>> GetNewArrivalsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .OrderByDescending(p => p.Id)
            .Take(10)
            .ToListAsync();
    }

    public async Task<long> AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product.Id;
    }

    public async Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity != null)
        {
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
