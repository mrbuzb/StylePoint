using StylePoint.Domain.Enums;

namespace StylePoint.Application.Dtos;

public class PaymentCreateDto
{
    public PaymentMethod Method { get; set; }
    public long OrderId { get; set; }
}