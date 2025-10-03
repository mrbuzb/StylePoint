using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IProductRepository
{
    Task<ICollection<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(long id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(long id);
}