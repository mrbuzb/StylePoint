using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<CategoryDto> CreateAsync(string name)
    {
        var category = new Category { Name = name };
        await _repo.AddAsync(category);

        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await _repo.DeleteAsync(id);
        return true;
    }

    public async Task<ICollection<CategoryDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto?> GetByIdAsync(long id)
    {
        var category = await _repo.GetByIdAsync(id);
        return category == null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(long id, string name)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null) throw new KeyNotFoundException($"Category with id {id} not found.");

        category.Name = name;
        await _repo.UpdateAsync(category);

        return MapToDto(category);
    }

    private static CategoryDto MapToDto(Category c) =>
        new CategoryDto
        {
            Id = c.Id,
            Name = c.Name
        };
}
