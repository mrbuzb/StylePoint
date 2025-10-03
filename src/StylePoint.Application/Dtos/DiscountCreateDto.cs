namespace StylePoint.Application.Dtos;

public class DiscountCreateDto
{
    public string Code { get; set; } = default!;
    public decimal Percentage { get; set; }
    public DateTime ExpiryDate { get; set; }
}