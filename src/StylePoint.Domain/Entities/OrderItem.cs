namespace StylePoint.Domain.Entities;

public class OrderItem
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}