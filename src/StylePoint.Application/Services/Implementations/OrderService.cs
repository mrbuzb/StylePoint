using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;

namespace StylePoint.Application.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ICartItemRepository _cartRepo;
    private readonly IOrderItemRepository _orderItemRepo;

    public OrderService(
        IOrderRepository orderRepo,
        ICartItemRepository cartRepo,
        IOrderItemRepository orderItemRepo)
    {
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _orderItemRepo = orderItemRepo;
    }

    public async Task<OrderDto> PlaceOrderAsync(long userId, OrderCreateDto dto)
    {
        var cartItems = await _cartRepo.GetUserCartAsync(userId);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        var total = cartItems.Sum(c => c.UnitPrice * c.Quantity);

        var order = new Order
        {
            UserId = userId,
            AddressId = dto.AddressId,
            TotalPrice = total,
            Status = OrderStatus.Pending
        };

        await _orderRepo.AddAsync(order);

        var orderItems = cartItems.Select(c => new OrderItem
        {
            Order = order,
            ProductVariantId = c.ProductVariantId,
            Quantity = c.Quantity,
            UnitPrice = c.UnitPrice
        }).ToList();

        await _orderItemRepo.AddRangeAsync(orderItems);

        await _cartRepo.ClearUserCartAsync(userId);

        return MapToDto(order, orderItems);
    }

    public async Task<OrderDto?> GetByIdAsync(long userId, long orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId) return null;

        return MapToDto(order, order.OrderItems);
    }

    public async Task<ICollection<OrderDto>> GetUserOrdersAsync(long userId)
    {
        var orders = await _orderRepo.GetByUserIdAsync(userId);
        return orders.Select(o => MapToDto(o, o.OrderItems)).ToList();
    }

    public async Task CancelOrderAsync(long userId, long orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null || order.UserId != userId)
            throw new InvalidOperationException("Order not found.");

        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Completed orders cannot be canceled.");

        order.Status = OrderStatus.Canceled;
        await _orderRepo.UpdateAsync(order);
    }

    private static OrderDto MapToDto(Order order, IEnumerable<OrderItem> items) =>
        new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            UserId = order.UserId,
            AddressId = order.AddressId,
            Items = items.Select(i => new OrderItemDto
            {
                ProductVariantId = i.ProductVariantId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
}
