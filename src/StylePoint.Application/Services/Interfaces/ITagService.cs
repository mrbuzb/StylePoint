using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface ITagService
{
    Task<ICollection<TagDto>> GetAllAsync();
    Task<TagDto?> GetByIdAsync(long id);
    Task<TagDto> CreateAsync(string name);
    Task<TagDto> UpdateAsync(long id, string name);
    Task DeleteAsync(long id);
}
