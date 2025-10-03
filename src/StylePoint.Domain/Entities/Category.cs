namespace StylePoint.Domain.Entities;

public class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}