using Backend.Models;

namespace Backend.Repositories;

public interface IShoppingCartRepository
{
    Task<ShoppingCart?> GetCartByCustomerIdAsync(long customerId);
    Task<List<CartItem>> GetAllItemsFromCartByCustomerId(long customerId);
    Task<bool> AddItemToCartAsync(long customerId, CartItem item);
    Task<bool> UpdateCartAsync(ShoppingCart cart);
    Task<bool> DeleteCartAsync(long customerId);
    Task<bool> DeleteCartItemAsync(long cartItemId,long customerId);
}