using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IDiscountService
{
    Task<decimal?> ValidateDiscountAsync(string code,long userId);
    Task<ICollection<DiscountDto>> GetAllActiveDiscountsAsync();
    Task<decimal> ApplyDiscountAsync(long userId, string code);
    Task<DiscountDto> CreateAsync(DiscountCreateDto dto);
    Task<DiscountDto> UpdateAsync(long id, DiscountUpdateDto dto);
    Task<bool> DeleteAsync(long id);
    Task<DiscountDto?> GetByCodeAsync(string code);
    Task<ICollection<DiscountDto>> GetActiveDiscountsAsync();
    Task<DiscountDto?> GetByIdAsync(long id);
    Task<ICollection<DiscountDto>> GetAllAsync();
}
