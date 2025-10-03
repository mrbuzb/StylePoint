namespace StylePoint.Domain.Entities;

public class Brand
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}