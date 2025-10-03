using Microsoft.AspNetCore.Http;

namespace StylePoint.Application.Dtos;

public class ProductCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public IFormFile Image { get; set; }
    public long CategoryId { get; set; }
    public long BrandId { get; set; }

    public ICollection<ProductVariantCreateDto> Variants { get; set; }
}