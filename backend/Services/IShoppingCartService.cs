namespace Backend.Services;
using Backend.Models;
using Backend.DTOs;
public interface IShoppingCartService 
{
    Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId);
    Task<bool> AddItemsAsync(long userId, CartItemDto items);
    Task<bool> UpdateCartAsync(ShoppingCartDto cart);
    Task<bool> DeleteCartAsync(long id);
}