using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;

namespace StylePoint.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }


    public async Task<PaymentDto> ProcessPaymentAsync(long userId, PaymentCreateDto dto)
    {
        var payment = new Payment
        {
            Amount = dto.Amount,
            Method = dto.Method,
            OrderId = dto.OrderId,
            Status = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        return MapToDto(payment, userId);
    }

    public async Task<PaymentDto?> GetByIdAsync(long userId, long paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.Order.UserId != userId)
            return null;

        return MapToDto(payment, userId);
    }

    public async Task<bool> RefundPaymentAsync(long userId, long paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.Order.UserId != userId)
            return false;

        if (payment.Status != PaymentStatus.Paid)
            return false;

        payment.Status = PaymentStatus.Refunded;
        await _paymentRepository.UpdateAsync(payment);

        return true;
    }

    public async Task<PaymentStatus> GetPaymentStatusAsync(long userId, long paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null || payment.Order.UserId != userId)
            throw new KeyNotFoundException("Payment not found or not accessible");

        return payment.Status;
    }

    private static PaymentDto MapToDto(Payment payment, long userId)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            Method = payment.Method,
            Status = payment.Status,
            OrderId = payment.OrderId,
            UserId = userId
        };
    }
}
