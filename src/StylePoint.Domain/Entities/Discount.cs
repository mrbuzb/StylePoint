namespace StylePoint.Domain.Entities;

public class Discount
{
    public long Id { get; set; }
    public string Code { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public long UsageLimit { get; set; }

    public ICollection<UserDiscount> RedeemedUsers { get; set; }
}

