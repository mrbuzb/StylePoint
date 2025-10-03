namespace StylePoint.Domain.Entities;

public class DeliveryAddress
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;

    public long UserId { get; set; }
    public User User { get; set; } = null!;
}
