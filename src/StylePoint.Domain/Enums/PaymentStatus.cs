namespace StylePoint.Domain.Enums;

public enum PaymentStatus
{
    Pending = 0,     // To‘lov kutilmoqda
    Paid = 1,        // To‘lov muvaffaqiyatli amalga oshirilgan
    Failed = 2,      // To‘lov amalga oshmadi
    Refunded = 3,    // Pul qaytarilgan
    Cancelled = 4    // To‘lov bekor qilingan
}
