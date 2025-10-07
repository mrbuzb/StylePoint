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
    private readonly IProductVariantRepository _productVariantRepo;

    public OrderService(
        IOrderRepository orderRepo,
        ICartItemRepository cartRepo,
        IOrderItemRepository orderItemRepo,
        IProductVariantRepository productVariantRepo)
    {
        _orderRepo = orderRepo;
        _cartRepo = cartRepo;
        _orderItemRepo = orderItemRepo;
        _productVariantRepo = productVariantRepo;
    }

    public async Task<OrderDto> PlaceOrderAsync(long userId, OrderCreateDto dto)
    {
        var cartItems = await _cartRepo.GetUserCartAsync(userId);
        if (!cartItems.Any())
            throw new InvalidOperationException("Cart is empty.");

        decimal total = 0;
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cartItems)
        {
            var variant = await _productVariantRepo.GetByIdAsync(cartItem.ProductVariantId);
            if (variant == null)
                throw new InvalidOperationException("Product not found.");

            if (cartItem.Quantity > variant.Stock)
                throw new InvalidOperationException(
                    $"'{variant.Size}' uchun yetarli stock mavjud emas. Qolgan: {variant.Stock}");

            // ❌ variant.Stock -= cartItem.Quantity;   // O'chirildi
            // ❌ await _productVariantRepo.UpdateAsync(variant);

            total += cartItem.UnitPrice * cartItem.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductVariantId = cartItem.ProductVariantId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.UnitPrice,
            });
        }

        var order = new Order
        {
            UserId = userId,
            AddressId = dto.AddressId,
            TotalPrice = total,
            Status = OrderStatus.Pending,
            OrderItems = orderItems
        };

        await _orderRepo.AddAsync(order);
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

        foreach (var item in order.OrderItems)
        {
            var variant = await _productVariantRepo.GetByIdAsync(item.ProductVariantId);
            if (variant != null)
            {
                variant.Stock += item.Quantity;
                await _productVariantRepo.UpdateAsync(variant);
            }
        }

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
