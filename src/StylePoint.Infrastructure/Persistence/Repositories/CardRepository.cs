using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using StylePoint.Application.Interfaces;
using StylePoint.Core.Errors;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Repositories;

public class CardRepository(AppDbContext _context) : ICardRepository
{
    public async Task<long> AddAsync(Card card)
    {
        await _context.Cards.AddAsync(card);
        await _context.SaveChangesAsync();
        return card.CardId;
    }

    public async Task<ICollection<Card>> GetAllAsync()
    {
        return await _context.Cards.ToListAsync();
    }

    public async Task<Card?> GetByIdAsync(long id)
    {
        return await _context.Cards.FirstOrDefaultAsync(x=>x.UserId == id);
    }

    public async Task<Card?> GetByNumberAsync(Guid cardNumber)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
    }

    public async Task<Card> GetByUserIdAsync(long id)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.UserId == id) ?? throw new EntityNotFoundException($"UserId : {id}");
    }

    public async Task<decimal> TopUpCardAsync(Guid cardNumber,long amount)
    {
        var card =  _context.Cards.FirstOrDefault(c => c.CardNumber == cardNumber);
        if(card == null)
            throw new InvalidOperationException("Card not found.");
        card.Balance += amount;
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
        return card.Balance;
    }

    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
    }
}
