using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IBrandRepository
{
    Task<ICollection<Brand>> GetAllAsync();
    Task<Brand?> GetByIdAsync(long id);
    Task AddAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(long id);
}