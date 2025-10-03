using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _repo;

    public BrandService(IBrandRepository repo)
    {
        _repo = repo;
    }

    public async Task<BrandDto> CreateAsync(string name)
    {
        var brand = new Brand { Name = name };
        await _repo.AddAsync(brand);

        return MapToDto(brand);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await _repo.DeleteAsync(id);
        return true;
    }

    public async Task<ICollection<BrandDto>> GetAllAsync()
    {
        var brands = await _repo.GetAllAsync();
        return brands.Select(MapToDto).ToList();
    }

    public async Task<BrandDto?> GetByIdAsync(long id)
    {
        var brand = await _repo.GetByIdAsync(id);
        return brand == null ? null : MapToDto(brand);
    }

    public async Task<BrandDto> UpdateAsync(long id, string name)
    {
        var brand = await _repo.GetByIdAsync(id);
        if (brand == null) throw new KeyNotFoundException($"Brand with id {id} not found.");

        brand.Name = name;
        await _repo.UpdateAsync(brand);

        return MapToDto(brand);
    }

    private static BrandDto MapToDto(Brand b) =>
        new BrandDto
        {
            Id = b.Id,
            Name = b.Name
        };
}
