namespace Backend.Services;

using Backend.DTOs;
using Backend.Models;

public interface IShoppingCartService
{
    Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId, CancellationToken token);
    Task<bool> AddItemsAsync(long userId, CartItemDto items);
    Task<bool> UpdateItemsAsync(long userid, CartItemDto item);
    Task<bool> UpdateCartAsync(ShoppingCartDto cart);
    Task<bool> DeleteCartAsync(long userId);
    Task<bool> DeleteCartItemAsync(long cartItemId, long userId);
}
