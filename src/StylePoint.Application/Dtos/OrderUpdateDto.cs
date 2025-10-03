using StylePoint.Domain.Enums;

namespace StylePoint.Application.Dtos;

public class OrderUpdateDto
{
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
}