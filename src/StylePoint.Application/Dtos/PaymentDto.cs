using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;

namespace StylePoint.Application.Dtos;

public class PaymentDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }

    public long OrderId { get; set; }
    public long UserId { get; set; }
    public long? CardId { get; set; }
}