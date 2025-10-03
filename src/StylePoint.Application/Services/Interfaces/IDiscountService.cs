using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IDiscountService
{
    Task<DiscountDto?> ApplyDiscountAsync(string code, long userId, decimal orderAmount);
    Task<bool> ValidateDiscountAsync(string code);
    Task<ICollection<DiscountDto>> GetAllActiveDiscountsAsync();

    Task<DiscountDto> CreateAsync(DiscountCreateDto dto);
    Task<DiscountDto> UpdateAsync(long id, DiscountUpdateDto dto);
    Task<bool> DeleteAsync(long id);
    Task<ICollection<DiscountDto>> GetActiveDiscountsAsync();
}
