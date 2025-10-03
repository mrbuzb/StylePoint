using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<ICollection<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(long id);
    Task<CategoryDto> CreateAsync(string name);
    Task<CategoryDto> UpdateAsync(long id, string name);
    Task<bool> DeleteAsync(long id);
}
