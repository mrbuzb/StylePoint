using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IBrandService  
{
    Task<ICollection<BrandDto>> GetAllAsync();
    Task<BrandDto?> GetByIdAsync(long id);
    Task<BrandDto> CreateAsync(string name);
    Task<BrandDto> UpdateAsync(long id, string name);
    Task<bool> DeleteAsync(long id);
}
