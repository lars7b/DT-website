namespace Backend.Repositories;

using Backend.Models;
using Dapper;
using Npgsql;

// handles cart_items and shopping_carts tables
public class ShoppingCartRepository
{
    private readonly NpgsqlConnection db;

    public ShoppingCartRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        db = new NpgsqlConnection(connectionString);
    }

    public async Task<ShoppingCart?> GetCartByUserIdAsync(long userId)
    {
        ShoppingCart? cart = await db.QueryFirstOrDefaultAsync<ShoppingCart>(
            "SELECT * FROM shopping_carts WHERE customer_id = @userId LIMIT 1;",
            new { userId }
        );
        return cart;
    }

    public async Task<List<CartItem>> GetAllItemsFromCartByUserId(long userId)
    {
        var items = await db.QueryAsync<CartItem>(
            $"SELECT * FROM cart_items AS items JOIN shopping_carts AS cart ON items.cart_id = cart.id JOIN users ON users.id = cart.customer_id WHERE users.id = @userId;",
            new { userId }
        );
        return items.ToList();
    }

    public async Task<bool> AddItem(CartItem entity)
    {
        string query =
            "INSERT INTO shopping_carts (cart_id,product_id,quantity) VALUES (@CartId,@PorductId,@Quantity);";
        int result = await db.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<bool> CreateCart(ShoppingCart cart)
    {
        string query = "INSERT INTO shopping_carts (customer_id) VALUES (@CustomerId);";
        int result = await db.ExecuteAsync(query, cart);
        return result > 0;
    }

    public async Task<ShoppingCart?> GetCartById(long id)
    {
        ShoppingCart? cart = await db.QueryFirstOrDefaultAsync<ShoppingCart>(
            $"SELECT * FROM shopping_carts WHERE id = @id",
            new { id }
        );
        return cart;
    }

    public async Task<CartItem?> GetItemById(long id)
    {
        CartItem? item = await db.QueryFirstOrDefaultAsync<CartItem>(
            $"SELECT * FROM cart_items WHERE id = @id",
            new { id }
        );
        return item;
    }

    public async Task<bool> UpdateItems(ShoppingCart cart)
    {
        int result= 0;
        for (int i = 0; i < cart.Items.Count; i++)
        {
            string query="UPDATE cart_items SET product_id = @ProductId, quantity = @Quantity WHERE id = @Id AND cart_id=@CartId;";
            result += await db.ExecuteAsync(query, cart.Items[i]);
        }
        return result > 0;
    }

    public async Task<bool> Delete(ShoppingCart cart)
    {
        string query = $"DELETE FROM shopping_carts WHERE id = @Id;";
        var result = await db.ExecuteAsync(query, new { cart.Id });
        return result > 0;
    }
}
