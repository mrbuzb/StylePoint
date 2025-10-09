using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;

    public DiscountService(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }


    public async Task<decimal> ApplyDiscountAsync(long userId, string code)
    {
        return await _discountRepository.ApplyDiscountAsync(userId, code);
    }

    public async Task<DiscountDto> CreateAsync(DiscountCreateDto dto)
    {
        var discount = new Discount
        {
            Code = dto.Code,
            Percentage = dto.Percentage,
            ExpiryDate = dto.ExpiryDate,
            UsageLimit = dto.UsageLimit,
            IsActive = true,
        };

        await _discountRepository.AddAsync(discount);

        return MapToDto(discount);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var discount = await _discountRepository.GetByIdAsync(id);
        await _discountRepository.DeleteAsync(id);
        return true;
    }

    public async Task<ICollection<DiscountDto>> GetActiveDiscountsAsync()
    {
        var all = await _discountRepository.GetAllAsync();
        var active = all
            .Where(d => d.ExpiryDate >= DateTime.UtcNow)
            .Select(MapToDto)
            .ToList();

        return active;
    }

    public async Task<ICollection<DiscountDto>> GetAllAsync()
    {
        var discounts = await _discountRepository.GetAllAsync();
        return discounts.Select(MapToDto).ToList();
    }

    public async Task<DiscountDto?> GetByIdAsync(long id)
    {
        var discount = await _discountRepository.GetByIdAsync(id);
        return discount == null ? null : MapToDto(discount);
    }

    public async Task<DiscountDto> UpdateAsync(long id, DiscountUpdateDto dto)
    {
        var discount = await _discountRepository.GetByIdAsync(id);
        if (discount == null) throw new InvalidOperationException("Discount not found.");

        discount.Code = dto.Code;
        discount.Percentage = dto.Percentage;
        discount.ExpiryDate = dto.ExpiryDate;

        await _discountRepository.UpdateAsync(discount);

        return MapToDto(discount);
    }

    public async Task<DiscountDto?> GetByCodeAsync(string code)
    {
        var discount = await _discountRepository.GetByCodeAsync(code);
        return discount == null ? null : MapToDto(discount);
    }

    private static DiscountDto MapToDto(Discount d) =>
        new DiscountDto
        {
            Id = d.Id,
            Code = d.Code,
            Percentage = d.Percentage,
            ExpiryDate = d.ExpiryDate,
            IsActive = d.ExpiryDate >= DateTime.UtcNow
        };

    public async Task<ICollection<DiscountDto>> GetAllActiveDiscountsAsync()
    {
        var discounts = await _discountRepository.GetAllAsync();

        return discounts
            .Where(d => d.ExpiryDate > DateTime.UtcNow)
            .Select(d => new DiscountDto
            {
                Id = d.Id,
                Code = d.Code,
                Percentage = d.Percentage,
                ExpiryDate = d.ExpiryDate
            })
            .ToList();
    }

    public async Task<decimal?> ValidateDiscountAsync(string code, long userId)
    {
        var discount = await _discountRepository.GetByCodeAsync(code);

        if (discount == null)
            return null;

        if (discount.ExpiryDate < DateTime.UtcNow)
            return null;

        if (discount.IsActive == false)
            return null;

        if (discount.RedeemedUsers.Any(ud => ud.UserId == userId))
            return null;

        if (discount.UsageLimit <= discount.RedeemedUsers.Count)
        {
            return null;
        }
        return discount.Percentage;
    }

}
