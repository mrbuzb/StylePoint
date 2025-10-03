namespace StylePoint.Application.Dtos;

public class OrderItemDto
{
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}