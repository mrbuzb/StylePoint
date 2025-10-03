namespace StylePoint.Domain.Entities;

public class Tag
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}