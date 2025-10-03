using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IOrderItemRepository
{
    Task<ICollection<OrderItem>> GetAllAsync();
    Task<OrderItem?> GetByIdAsync(long id);
    Task AddAsync(OrderItem item);
    Task UpdateAsync(OrderItem item);
    Task DeleteAsync(long id);
    Task AddRangeAsync(ICollection<OrderItem> items);
}