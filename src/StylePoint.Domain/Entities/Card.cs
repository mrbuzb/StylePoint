namespace StylePoint.Domain.Entities;

public class Card
{
    public long CardId { get; set; }

    public Guid CardNumber { get; set; }
    public decimal Balance { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }

    public ICollection<Payment> Payments { get; set; }
}
