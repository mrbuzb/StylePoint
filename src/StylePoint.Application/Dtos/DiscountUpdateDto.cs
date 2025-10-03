namespace StylePoint.Application.Dtos;

public class DiscountUpdateDto
{
    public string Code { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }
}