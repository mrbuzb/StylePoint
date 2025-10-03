namespace StylePoint.Domain.Entities;

public class ProductVariant
{
    public long Id { get; set; }
    public string Size { get; set; } = null!; // S, M, L, XL, XXL
    public string Color { get; set; } = null!;
    public int Stock { get; set; }
    public decimal Price { get; set; }

    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;
}