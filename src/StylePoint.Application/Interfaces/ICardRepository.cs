using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface ICardRepository
{
    Task<ICollection<Card>> GetAllAsync();
    Task<Card?> GetByIdAsync(long id);
    Task<Card> GetByUserIdAsync(long id);
    Task<Card?> GetByNumberAsync(Guid cardNumber);
    Task<decimal> TopUpCardAsync(Guid cardNumber,long amount);
    Task<long> AddAsync(Card card);
    Task UpdateAsync(Card card);
}
