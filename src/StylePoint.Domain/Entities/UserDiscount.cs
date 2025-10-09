namespace StylePoint.Domain.Entities;

public class UserDiscount
{
    public long UserId { get; set; }
    public User User { get; set; }

    public long DiscountId { get; set; }
    public Discount Discount { get; set; }

    public DateTime UsedAt { get; set; }
}
