using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface ICartItemRepository
{
    Task<ICollection<CartItem>> GetAllAsync();
    Task<CartItem?> GetByIdAsync(long id);
    Task AddAsync(CartItem item);
    Task UpdateAsync(CartItem item);
    Task DeleteAsync(long id);
    Task<ICollection<CartItem>> GetUserCartAsync(long userId);
    Task ClearUserCartAsync(long userId);
}
