namespace StylePoint.Application.Dtos;

public class ProductCreateDto
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public long CategoryId { get; set; }
    public long BrandId { get; set; }
}