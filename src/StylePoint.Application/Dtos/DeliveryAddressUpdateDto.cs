namespace StylePoint.Application.Dtos;

public class DeliveryAddressUpdateDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}