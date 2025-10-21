namespace StylePoint.Application.Dtos;

public class ProductUpdateDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string SecretCode { get; set; }
    public decimal? Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public long? CategoryId { get; set; }
    public long? BrandId { get; set; }
}