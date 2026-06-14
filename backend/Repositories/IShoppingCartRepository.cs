using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public interface IShoppingCartRepository
{
    public Task<ShoppingCart?> GetCartByUserIdAsync(
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
    public Task<bool> AddItemToCartAsync(long userId, CartItem item);

    public Task<ShoppingCart?> GetCartById(long id,CancellationToken token = default);
    public Task<CartItem?> GetItemById(long id,CancellationToken token = default);

    public Task<bool> UpdateItems(ShoppingCart cart);
    public Task<bool> UpdateItems(CartItem items);

    public Task<bool> DeleteCartAsync(ShoppingCart cart);

    public Task<bool> DeleteItemAsync(long cartItemId, long userId);
}
