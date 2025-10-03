using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IOrderRepository
{
    Task<ICollection<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(long id);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(long id);
    Task<ICollection<Order>> GetByUserIdAsync(long userId);
}
