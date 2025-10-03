using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;

namespace StylePoint.Application.Services.Implementations;

public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;

    public DiscountService(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }

    public async Task<DiscountDto?> ApplyDiscountAsync(string code, long userId, decimal orderAmount)
    {
        var discount = (await _discountRepository.GetAllAsync())
            .FirstOrDefault(d => d.Code == code && d.ExpiryDate > DateTime.UtcNow);

        if (discount == null)
            return null;

        decimal discountedAmount = orderAmount - (orderAmount * discount.Percentage / 100m);
        if (discountedAmount < 0)
            discountedAmount = 0;

        return new DiscountDto
        {
            Id = discount.Id,
            Code = discount.Code,
            Percentage = discount.Percentage,
            ExpiryDate = discount.ExpiryDate,
            FinalPrice = discountedAmount
        };
    }

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

    public async Task<bool> ValidateDiscountAsync(string code)
    {
        var discounts = await _discountRepository.GetAllAsync();
        var discount = discounts.FirstOrDefault(d => d.Code == code);

        if (discount == null)
            return false;

        if (discount.ExpiryDate < DateTime.UtcNow)
            return false;

        return true;
    }
}
