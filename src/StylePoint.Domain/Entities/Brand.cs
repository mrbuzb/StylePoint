namespace StylePoint.Domain.Entities;

public class Brand
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<Product> Products { get; set; }
}