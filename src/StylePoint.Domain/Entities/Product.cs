namespace StylePoint.Domain.Entities;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string ImageUrl { get; set; } = null!;

    public long CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public long BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}