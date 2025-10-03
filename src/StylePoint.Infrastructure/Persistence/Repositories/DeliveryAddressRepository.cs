using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class DeliveryAddressRepository : IDeliveryAddressRepository
{
    private readonly AppDbContext _context;
    public DeliveryAddressRepository(AppDbContext context) => _context = context;

    public async Task<ICollection<DeliveryAddress>> GetAllAsync()
        => await _context.DeliveryAddresses.ToListAsync();

    public async Task<DeliveryAddress?> GetByIdAsync(long id)
        => await _context.DeliveryAddresses.FindAsync(id);

    public async Task AddAsync(DeliveryAddress address)
    {
        await _context.DeliveryAddresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(DeliveryAddress address)
    {
        _context.DeliveryAddresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await _context.DeliveryAddresses.FindAsync(id);
        if (entity != null)
        {
            _context.DeliveryAddresses.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
