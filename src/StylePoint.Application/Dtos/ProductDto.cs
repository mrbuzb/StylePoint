namespace StylePoint.Application.Dtos;

public class ProductDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string SecretCode { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string BrandName { get; set; } = null!;
    public ICollection<ProductVariantDto> Variants { get; set; } = new List<ProductVariantDto>();
    public ICollection<string> Tags { get; set; } = new List<string>();
}