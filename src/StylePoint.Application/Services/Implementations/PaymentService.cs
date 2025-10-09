using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Core.Errors;
using StylePoint.Domain.Entities;
using StylePoint.Domain.Enums;

namespace StylePoint.Application.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProductVariantRepository _productVariantRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly ICardRepository _cardRepo;
    private readonly IUserRepository _userRepo;
    public PaymentService(IPaymentRepository paymentRepository, IProductVariantRepository productVariantRepo, IOrderRepository orderRepo, ICardRepository cardRepo, IUserRepository userRepo)
    {
        _paymentRepository = paymentRepository;
        _productVariantRepo = productVariantRepo;
        _orderRepo = orderRepo;
        _cardRepo = cardRepo;
        _userRepo = userRepo;
    }

    public async Task<PaymentDto> ProcessTelegramPaymentAsync(long telegramId, PaymentCreateDto dto)
    {
        // Telegram userni topish
        var user = await _userRepo.GetWithOrdersAndCardByTelegramIdAsync(telegramId);



        if (user == null)
            throw new InvalidOperationException("Telegram user not found.");

        var order = user.Orders.FirstOrDefault(o => o.Id == dto.OrderId);

        var amount = order.TotalPrice;
        if (dto.Discount is not null)
        {
            amount -= (order.TotalPrice * dto.Discount.Value / 100);
        }
        if (order == null)
            throw new InvalidOperationException("Order not found for this user.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order already processed.");

        if (dto.Method == PaymentMethod.Card)
        {
            var card = user.Card;
            if (card == null)
                throw new NotAllowedException("Card not found for this user.");

            if (card.Balance < amount)
                throw new NotAllowedException("Card balance is insufficient");
        }

        foreach (var item in order.OrderItems)
        {
            var variant = await _productVariantRepo.GetByIdAsync(item.ProductVariantId);
            if (variant == null)
                throw new InvalidOperationException("Product variant not found.");

            if (item.Quantity > variant.Stock)
                throw new InvalidOperationException(
                    $"'{variant.Size}' uchun yetarli stock mavjud emas. Qolgan: {variant.Stock}");

            variant.Stock -= item.Quantity;
            await _productVariantRepo.UpdateAsync(variant);
        }

        var payment = new Payment();

        

        if (dto.Method == PaymentMethod.Card)
        {
            var card = user.Card!;
            card.Balance -= amount;
            await _cardRepo.UpdateAsync(card);

            payment = new Payment
            {
                Amount = amount,
                Method = dto.Method,
                OrderId = dto.OrderId,
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                CardId = card.CardId,
            };

            await _paymentRepository.AddAsync(payment);
        }
        else
        {
            payment = new Payment
            {
                Amount = amount,
                Method = dto.Method,
                OrderId = dto.OrderId,
                Status = PaymentStatus.CashPay,
                PaidAt = DateTime.UtcNow,
            };

            await _paymentRepository.AddAsync(payment);
        }

        order.Status = OrderStatus.Completed;
        await _orderRepo.UpdateAsync(order);

        return MapToDto(payment, user.UserId);
    }

    public async Task<PaymentDto> ProcessPaymentAsync(long userId, PaymentCreateDto dto)
    {
        var card = await _cardRepo.GetByIdAsync(userId);

        var order = await _orderRepo.GetByIdAsync(dto.OrderId);

        if (order == null || order.UserId != userId)
            throw new InvalidOperationException("Order not found.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Order already processed.");

        if (dto.Method == PaymentMethod.Card)
        {
            if (card == null || card.UserId != userId)
                throw new NotAllowedException("Card not found or not accessible");
            if (card.Balance < order.TotalPrice)
                throw new NotAllowedException("Card balance is insufficient");
        }

        foreach (var item in order.OrderItems)
        {
            var variant = await _productVariantRepo.GetByIdAsync(item.ProductVariantId);
            if (variant == null)
                throw new InvalidOperationException("Product variant not found.");

            if (item.Quantity > variant.Stock)
                throw new InvalidOperationException(
                    $"'{variant.Size}' uchun yetarli stock mavjud emas. Qolgan: {variant.Stock}");

            variant.Stock -= item.Quantity;
            await _productVariantRepo.UpdateAsync(variant);
        }
        var payment = new Payment();
        if (dto.Method == PaymentMethod.Card)
        {
            

            
            card.Balance -= order.TotalPrice;
            await _cardRepo.UpdateAsync(card);

            payment = new Payment
            {
                Amount = order.TotalPrice,
                Method = dto.Method,
                OrderId = dto.OrderId,
                Status = PaymentStatus.Paid,
                PaidAt = DateTime.UtcNow,
                CardId = card.CardId,
            };

            await _paymentRepository.AddAsync(payment);

            order.Status = OrderStatus.Completed;
            await _orderRepo.UpdateAsync(order);

            return MapToDto(payment, userId);
        }

        payment = new Payment
        {
            Amount = order.TotalPrice,
            Method = dto.Method,
            OrderId = dto.OrderId,
            Status = PaymentStatus.CashPay,
            PaidAt = DateTime.UtcNow,
        };

        await _paymentRepository.AddAsync(payment);

        order.Status = OrderStatus.Processing;
        await _orderRepo.UpdateAsync(order);

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
            UserId = userId,
            CardId = payment.CardId,
        };
    }

    public async Task<decimal> TopUpCardAsync(Guid cardNumber, long amount)
    {
        return await _cardRepo.TopUpCardAsync(cardNumber, amount);
    }
}
