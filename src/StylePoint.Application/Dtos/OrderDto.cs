using StylePoint.Domain.Enums;

namespace StylePoint.Application.Dtos;

public class OrderDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }

    public long UserId { get; set; }
    public long? AddressId { get; set; }

    public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}