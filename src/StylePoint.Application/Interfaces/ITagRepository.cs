using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface ITagRepository
{
    Task<ICollection<Tag>> GetAllAsync();
    Task<Tag?> GetByIdAsync(long id);
    Task AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(long id);
}