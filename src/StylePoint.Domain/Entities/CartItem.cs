namespace StylePoint.Domain.Entities;

public class CartItem
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;
}