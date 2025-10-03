using StylePoint.Domain.Entities;

namespace StylePoint.Application.Interfaces;

public interface IDeliveryAddressRepository
{
    Task<ICollection<DeliveryAddress>> GetAllAsync();
    Task<DeliveryAddress?> GetByIdAsync(long id);
    Task AddAsync(DeliveryAddress address);
    Task UpdateAsync(DeliveryAddress address);
    Task DeleteAsync(long id);
}