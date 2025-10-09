namespace StylePoint.Application.Dtos;

public class DiscountCreateDto
{
    public string Code { get; set; }
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public long UsageLimit { get; set; }
}