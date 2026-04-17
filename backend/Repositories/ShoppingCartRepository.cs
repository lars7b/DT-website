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
        var cart = await db.QueryFirstOrDefaultAsync<ShoppingCart>(
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
        var result = await db.ExecuteAsync(query, entity);
        return result > 0;
    }

    public async Task<bool> CreateCart(ShoppingCart cart)
    {
        string query = "INSERT INTO shopping_carts (customer_id) VALUES (@CustomerId);";
        var result = await db.ExecuteAsync(query, cart);
        return result > 0;
    }

    public async Task<ShoppingCart?> GetCartById(long id)
    {
        var cart = await db.QueryFirstOrDefaultAsync<ShoppingCart>(
            $"SELECT * FROM shopping_carts WHERE id = @id",
            new { id }
        );
        return cart;
    }

    public async Task<CartItem?> GetItemById(long id)
    {
        var cart = await db.QueryFirstOrDefaultAsync<ShoppingCart>(
            $"SELECT * FROM cart_items WHERE id = @id",
            new { id }
        );
        return cart;
    }

    // public async Task<ShoppingCart>
    public async Task<bool> Update(ShoppingCart cart)
    {
                throw new NotImplementedException();
        return false;
    }

    public async Task<bool> Delete(ShoppingCart cart)
    {
                throw new NotImplementedException();
        return false;
    }
}
