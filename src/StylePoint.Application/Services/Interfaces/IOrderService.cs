using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(long userId, OrderCreateDto dto);
    Task<OrderDto?> GetByIdAsync(long userId, long orderId);
    Task<ICollection<OrderDto>> GetUserOrdersAsync(long userId);
    Task CancelOrderAsync(long userId, long orderId);
}
