using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class TagService : ITagService
{
    private readonly ITagRepository _repo;

    public TagService(ITagRepository repo)
    {
        _repo = repo;
    }

    public async Task<TagDto> CreateAsync(string name)
    {
        var tag = new Tag { Name = name };
        await _repo.AddAsync(tag);

        return MapToDto(tag);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        await _repo.DeleteAsync(id);
        return true;
    }

    public async Task<ICollection<TagDto>> GetAllAsync()
    {
        var tags = await _repo.GetAllAsync();
        return tags.Select(MapToDto).ToList();
    }

    public async Task<TagDto?> GetByIdAsync(long id)
    {
        var tag = await _repo.GetByIdAsync(id);
        return tag == null ? null : MapToDto(tag);
    }

    public async Task<TagDto> UpdateAsync(long id, string name)
    {
        var tag = await _repo.GetByIdAsync(id);
        if (tag == null) throw new KeyNotFoundException($"Tag with id {id} not found.");

        tag.Name = name;
        await _repo.UpdateAsync(tag);

        return MapToDto(tag);
    }

    private static TagDto MapToDto(Tag t) =>
        new TagDto
        {
            Id = t.Id,
            Name = t.Name
        };
}
