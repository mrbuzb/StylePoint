using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IProductRepository
{
    Task<ICollection<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(long id);
    Task<ICollection<Product>> SearchAsync(string keyword);
    Task<ICollection<Product>> GetByCategoryAsync(long categoryId);
    Task<ICollection<Product>> GetByBrandAsync(long brandId);
    Task<ICollection<Product>> GetByTagAsync(long tagId);
    Task<ICollection<Product>> GetBestSellersAsync();
    Task<ICollection<Product>> GetNewArrivalsAsync();
    Task<long> AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(long id);
}