using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface ICategoryRepository
{
    Task<ICollection<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(long id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(long id);
}