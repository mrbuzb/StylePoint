using StylePoint.Application.Dtos;

namespace StylePoint.Application.Services.Interfaces;

public interface IDeliveryAddressService
{
    Task<ICollection<DeliveryAddressDto>> GetUserAddressesAsync(long userId);
    Task AddAddressAsync(long userId, DeliveryAddressCreateDto dto);
    Task UpdateAddressAsync(long userId, DeliveryAddressUpdateDto dto);
    Task DeleteAddressAsync(long userId, long addressId);
}
