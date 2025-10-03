using StylePoint.Domain.Enums;

namespace StylePoint.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public long? AddressId { get; set; }
    public DeliveryAddress? Address { get; set; }
}