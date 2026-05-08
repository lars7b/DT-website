using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public interface IShoppingCartRepository
{
    // Task<ShoppingCart?> GetCartByCustomerIdAsync(long customerId);
    // Task<List<CartItem>> GetAllItemsFromCartByCustomerId(long customerId);
    // Task<bool> AddItemToCartAsync(long customerId, CartItem item);
    // Task<bool> UpdateCartAsync(ShoppingCart cart);
    // Task<bool> DeleteCartAsync(long customerId);
    // Task<bool> DeleteCartItemAsync(long cartItemId,long customerId);

    public Task<ShoppingCart?> GetCartByCustomerIdAsync(
        long userId,
        NpgsqlConnection? con = null,
        NpgsqlTransaction? transaction = null, CancellationToken token = default
    );

    public Task<List<CartItem>> GetAllItemsFromCartByCustomerId(
        long userId,
        CancellationToken token
    );

    public Task<bool> AddItem(
        CartItem entity,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    );

    // public Task<ShoppingCart> CreateCart(
    //     ShoppingCart cart,
    //     NpgsqlConnection? connection = null,
    //     NpgsqlTransaction? transaction = null
    // );

    public Task<bool> AddItemToCartAsync(long userId, CartItem item);

    public Task<ShoppingCart?> GetCartById(long id);
    public Task<CartItem?> GetItemById(long id);

    public Task<bool> UpdateItems(ShoppingCart cart);
    public Task<bool> UpdateItems(CartItem items);

    public Task<bool> DeleteCartAsync(ShoppingCart cart);

    public Task<bool> DeleteItemAsync(long cartItemId, long userId);
}
