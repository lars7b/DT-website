namespace Backend.Repositories;

using Backend.Models;
using Dapper;
using Npgsql;

/// <summary>
/// handles cart_items and shopping_carts tables
/// cart_items has the following attributes : id, cart_id,product_id,quantity
/// shopping_carts has the following attributes : id, customer_id
/// </summary>
public class ShoppingCartRepository : IShoppingCartRepository
{
    //https://dappertutorial.net/dapper-transaction-third-party-library
    // https://www.conradakunga.com/blog/dapper-part-10-handling-cancellations/
    private readonly string _connectionString;

    public ShoppingCartRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public async Task<ShoppingCart?> GetCartByCustomerIdAsync(long userId,NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null, CancellationToken token = default)
    {
        if (connection == null)
        {
            connection = new NpgsqlConnection(_connectionString);
        }
        ShoppingCart? cart = await connection.QueryFirstOrDefaultAsync<ShoppingCart>(
            @"SELECT sc.* FROM shopping_carts AS sc
            JOIN customers AS c ON sc.customer_id = c.id
            WHERE c.user_id = @userId 
            LIMIT 1;",
            // "SELECT * FROM get_all_items_from_cart;",
            new { userId },transaction
        );
        return cart;
    }

    public async Task<List<CartItem>> GetAllItemsFromCartByCustomerId(
        long userId,
        CancellationToken token
    )
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        IEnumerable<CartItem> items = await connection.QueryAsync<CartItem>(
            """
            SELECT items.id,items.cart_id AS cartid,items.product_id AS productid,items.quantity
            FROM cart_items AS items
            JOIN shopping_carts AS carts ON items.cart_id = carts.id 
            JOIN customers ON customers.id = carts.customer_id 
            WHERE customers.user_id = @userId;
            """,
            new { userId }
        );
        return items.ToList();
    }

    public async Task<bool> AddItem(
        CartItem entity,
        NpgsqlConnection? con = null,
        NpgsqlTransaction? transaction = null
    )
    {
        string query =
            "INSERT INTO cart_items (cart_id,product_id,quantity) VALUES (@CartId,@ProductId,@Quantity);";

        if (con == null)
        {
            con = new NpgsqlConnection(_connectionString);
        }
        int result = await con.ExecuteAsync(query, entity,transaction);
        return result > 0;
    }

    public async Task<ShoppingCart> CreateCart(
        ShoppingCart cart,
        NpgsqlConnection? con = null,
        NpgsqlTransaction? transaction = null
    )
    {
        string query =
            @"INSERT INTO shopping_carts (customer_id) VALUES (@CustomerId)
            RETURNING *;";
        if (con == null)
        {
            con = new NpgsqlConnection(_connectionString);
        }
        ShoppingCart result = await con.QuerySingleAsync<ShoppingCart>(query, cart,transaction);
        return result;
    }

    /// <summary>
    /// creates a cart for the user/customer if it doesn't exist and returns the cart id
    /// </summary>
    /// <param name="user_id">the id of the user that made this request, they have to be a customer for this to work</param>
    /// <param name="con">connection</param>
    /// <param name="transaction">transaction that its running on</param>
    /// <returns>the id of the created cart</returns>
    public async Task<long> CreateCart(
        long user_id,
        NpgsqlConnection? con = null,
        NpgsqlTransaction? transaction = null
    )
    {
        string query =
            @"INSERT INTO shopping_carts (customer_id)
            Select id from customers 
            where user_id = @User_id
            RETURNING id;";
        if (con == null)
        {
            con = new NpgsqlConnection(_connectionString);
        }
        long result = await con.QuerySingleAsync<long>(query, new {user_id},transaction);
        return result;
    }

    public async Task<bool> AddItemToCartAsync(long userId, CartItem item)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            ShoppingCart? cart = await GetCartByCustomerIdAsync(userId,connection,transaction);
            long cartid = 0;
            if (cart == null)
            {
                cartid = await CreateCart(userId,connection,transaction);
            }
            else{cartid=cart.Id;}
            // // // TODO check if product exists + fix logic
            // // for(int i=0;i<cart.Items.Count;i++)
            // // {
            // //     if(cart.Items[i].ProductId == items.ProductId){
            // //         cart.Items[i].Quantity = items.Quantity; //could be +=
            // //         return await _shoppingCartRepository.UpdateItems(cart.Items[i]);
            // //     }
            // // }
            item.CartId = cartid;
            bool result = await AddItem(item, connection,transaction);
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ShoppingCart?> GetCartById(long id,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        ShoppingCart? cart = await connection.QueryFirstOrDefaultAsync<ShoppingCart>(
            "SELECT * FROM shopping_carts WHERE id = @id",
            new { id }
        );
        return cart;
    }

    public async Task<CartItem?> GetItemById(long id,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        CartItem? item = await connection.QueryFirstOrDefaultAsync<CartItem>(
            "SELECT * FROM cart_items WHERE id = @id",
            new { id }
        );
        return item;
    }

    public async Task<bool> UpdateItems(ShoppingCart cart)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        int result = 0;
        for (int i = 0; i < cart.Items.Count; i++)
        {
            string query =
                "UPDATE cart_items SET product_id = @ProductId, quantity = @Quantity WHERE id = @Id AND cart_id=@CartId;";
            result += await connection.ExecuteAsync(query, cart.Items[i]); //could use overload
        }
        return result > 0;
    }

    public async Task<bool> UpdateItems(CartItem items)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query =
            "UPDATE cart_items SET product_id = @ProductId, quantity = @Quantity WHERE id = @Id AND cart_id=@CartId;";
        int result = await connection.ExecuteAsync(query, items);
        return result > 0;
    }

    public async Task<bool> DeleteCartAsync(ShoppingCart cart) // can send cartid and userid then check if users cart or if user admin
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = "DELETE FROM shopping_carts WHERE id = @Id;";
        int result = await connection.ExecuteAsync(query, new { cart.Id });
        return result > 0;
    }

    public async Task<bool> DeleteItemAsync(long cartItemId, long userId)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = """
            DELETE FROM cart_items AS ci 
            USING shopping_carts AS sc
            JOIN customers AS c ON sc.customer_id = c.id
            JOIN users AS u ON c.user_id=u.id
            WHERE sc.id = ci.cart_id  AND ci.id = @Id AND (u.id =@Userid OR u.role = 'Admin');
            """;
        int result = await connection.ExecuteAsync(query, new { Id = cartItemId, Userid = userId });
        return result > 0;
    }
}
