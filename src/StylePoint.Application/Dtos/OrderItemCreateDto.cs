namespace StylePoint.Application.Dtos;

public class OrderItemCreateDto
{
    public long OrderId { get; set; }
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}