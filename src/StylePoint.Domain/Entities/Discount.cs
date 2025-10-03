namespace StylePoint.Domain.Entities;

public class Discount
{
    public long Id { get; set; }
    public string Code { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }
}

