namespace StylePoint.Application.Dtos;

public class DiscountDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }

    public decimal? FinalPrice { get; set; } 
}