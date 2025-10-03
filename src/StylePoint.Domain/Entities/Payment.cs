using StylePoint.Domain.Enums;

namespace StylePoint.Domain.Entities;

public class Payment
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
}