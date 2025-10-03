namespace StylePoint.Application.Dtos;

public class CartItemDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}