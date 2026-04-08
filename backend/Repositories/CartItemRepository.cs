namespace Backend.Repositories;

using Backend.Models;
using Npgsql;

public class CartItemRepository :  RepositoryBase<CartItem>
{
    private static readonly string _table = "shopping_carts";
    private static CartItem _map(NpgsqlDataReader reader)
    {
        return new CartItem
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            CartId = reader.GetInt64(reader.GetOrdinal("cart_id")),
            ProductId = reader.GetInt64(reader.GetOrdinal("product_id")),
            Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
        };
    }
    private static readonly string _attributes = "cart_id, product_id, quantity";
    private static readonly Dictionary<string, string> _reverseMap = new Dictionary<string, string>
    {
        {"CartId", "cart_id"},
        {"ProductId", "product_id"},
        {"Quantity", "quantity"}
    };
    public CartItemRepository(IConfiguration configuration)
        : base(configuration, _table, _map, _attributes,_reverseMap) { }


}