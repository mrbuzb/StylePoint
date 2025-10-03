namespace StylePoint.Application.Dtos;

public class ProductVariantUpdateDto
{
    public string Size { get; set; } = default!;
    public string Color { get; set; } = default!;
    public int Stock { get; set; }
    public decimal Price { get; set; }
}