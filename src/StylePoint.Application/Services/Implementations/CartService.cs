using StylePoint.Application.Dtos;
using StylePoint.Application.Interfaces;
using StylePoint.Application.Services.Interfaces;
using StylePoint.Domain.Entities;

namespace StylePoint.Application.Services.Implementations;

public class CartService : ICartService
{
    private readonly ICartItemRepository _cartRepository;
    private readonly IProductVariantRepository _variantRepository;

    public CartService(ICartItemRepository cartRepository, IProductVariantRepository variantRepository)
    {
        _cartRepository = cartRepository;
        _variantRepository = variantRepository;
    }

    public async Task AddToCartAsync(long userId, CartItemCreateDto dto)
    {
        var variant = await _variantRepository.GetByIdAsync(dto.ProductVariantId);
        if (variant == null)
            throw new Exception("Product variant not found");

        var userCartItems = await _cartRepository.GetAllAsync();
        var existingItem = userCartItems
            .FirstOrDefault(x => x.UserId == userId && x.ProductVariantId == dto.ProductVariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += dto.Quantity;
            existingItem.UnitPrice = variant.Price;
            await _cartRepository.UpdateAsync(existingItem);
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = userId,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity,
                UnitPrice = variant.Price
            };

            await _cartRepository.AddAsync(cartItem);
        }
    }

    public async Task<decimal> CalculateSubtotalAsync(long userId)
    {
        var cartItems = await _cartRepository.GetAllAsync();
        var userItems = cartItems.Where(x => x.UserId == userId);

        return userItems.Sum(x => x.UnitPrice * x.Quantity);
    }

    public async Task ClearCartAsync(long userId)
    {
        var cartItems = await _cartRepository.GetAllAsync();
        var userItems = cartItems.Where(x => x.UserId == userId);

        foreach (var item in userItems)
        {
            await _cartRepository.DeleteAsync(item.Id);
        }
    }

    public async Task<ICollection<CartItemDto>> GetUserCartAsync(long userId)
    {
        var cartItems = await _cartRepository.GetAllAsync();
        var userItems = cartItems.Where(x => x.UserId == userId);

        return userItems.Select(item => new CartItemDto
        {
            Id = item.Id,
            ProductVariantId = item.ProductVariantId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.UnitPrice * item.Quantity
        }).ToList();
    }

    public async Task RemoveFromCartAsync(long userId, long cartItemId)
    {
        var item = await _cartRepository.GetByIdAsync(cartItemId);
        if (item == null || item.UserId != userId)
            throw new Exception("Cart item not found or access denied");

        await _cartRepository.DeleteAsync(cartItemId);
    }

    public async Task UpdateQuantityAsync(long userId, long cartItemId, int quantity)
    {
        var item = await _cartRepository.GetByIdAsync(cartItemId);
        if (item == null || item.UserId != userId)
            throw new Exception("Cart item not found or access denied");

        if (quantity <= 0)
        {
            await _cartRepository.DeleteAsync(cartItemId);
        }
        else
        {
            item.Quantity = quantity;
            await _cartRepository.UpdateAsync(item);
        }
    }
}
