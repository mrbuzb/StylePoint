using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface ICartService
{
    Task<ICollection<CartItemDto>> GetUserCartAsync(long userId);
    Task AddToCartAsync(long userId, CartItemCreateDto dto);
    Task UpdateQuantityAsync(long userId, long cartItemId, int quantity);
    Task RemoveFromCartAsync(long userId, long cartItemId);
    Task<decimal> CalculateSubtotalAsync(long userId);
    Task ClearCartAsync(long userId);
}
