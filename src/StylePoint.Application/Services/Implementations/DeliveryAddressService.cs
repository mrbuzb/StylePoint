using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class DeliveryAddressService : IDeliveryAddressService
{
    private readonly IDeliveryAddressRepository _addressRepository;

    public DeliveryAddressService(IDeliveryAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task AddAddressAsync(long userId, DeliveryAddressCreateDto dto)
    {
        var address = new DeliveryAddress
        {
            UserId = userId,
            FullName = dto.FullName,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country
        };

        await _addressRepository.AddAsync(address);
    }

    public async Task DeleteAddressAsync(long userId, long addressId)
    {
        var address = await _addressRepository.GetByIdAsync(addressId);
        if (address == null || address.UserId != userId)
            throw new Exception("Address not found or access denied");

        await _addressRepository.DeleteAsync(addressId);
    }

    public async Task<ICollection<DeliveryAddressDto>> GetUserAddressesAsync(long userId)
    {
        var addresses = await _addressRepository.GetAllAsync();
        var userAddresses = addresses.Where(a => a.UserId == userId);

        return userAddresses.Select(a => new DeliveryAddressDto
        {
            Id = a.Id,
            FullName = a.FullName,
            Address = a.Address,
            City = a.City,
            PostalCode = a.PostalCode,
            Country = a.Country
        }).ToList();
    }

    public async Task UpdateAddressAsync(long userId, DeliveryAddressUpdateDto dto)
    {
        var address = await _addressRepository.GetByIdAsync(dto.Id);
        if (address == null || address.UserId != userId)
            throw new Exception("Address not found or access denied");

        address.FullName = dto.FullName;
        address.Address = dto.Address;
        address.City = dto.City;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;

        await _addressRepository.UpdateAsync(address);
    }
}
