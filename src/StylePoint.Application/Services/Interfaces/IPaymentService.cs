using StylePoint.Application.Dtos;
using StylePoint.Domain.Enums;

namespace StylePoint.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<decimal> TopUpCardAsync(Guid cardNumber, long amount);
    Task<PaymentDto> ProcessPaymentAsync(long userId, PaymentCreateDto dto);
    Task<PaymentDto?> GetByIdAsync(long userId, long paymentId);
    Task<bool> RefundPaymentAsync(long userId, long paymentId);
    Task<PaymentStatus> GetPaymentStatusAsync(long userId, long paymentId);
    Task<PaymentDto> ProcessTelegramPaymentAsync(long telegramId, PaymentCreateDto dto);
}
