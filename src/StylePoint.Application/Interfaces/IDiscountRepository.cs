using StylePoint.Application.Dtos;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IDiscountRepository
{
    Task<ICollection<Discount>> GetAllAsync();
    Task<Discount?> GetByIdAsync(long id);
    Task<decimal> ApplyDiscountAsync(long userId, string code);
    Task AddAsync(Discount discount);
    Task UpdateAsync(Discount discount);
    Task DeleteAsync(long id);
    Task<Discount?> GetByCodeAsync(string code);
}