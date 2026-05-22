using System;
using System.Threading.Tasks;
using Backend.Models;
using Npgsql;
using StackExchange.Redis;

namespace Backend.Repositories;

public class ShoppingCartRepositoryRedis : IShoppingCartRepository
{
    private readonly string _connectionString;

    public ShoppingCartRepositoryRedis(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("RedisDefaultConnection")
            ?? throw new InvalidOperationException("Redis DB Connection missing");
        // var redis = ConnectionMultiplexer.ConnectAsync(_connectionString);
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        ///
        IDatabase db = redis.GetDatabase();
    }

    // https://stackexchange.github.io/StackExchange.Redis/Basics.html
    // https://www.c-sharpcorner.com/article/getting-started-with-redis-in-net-core-applications/
    public Task<ShoppingCart?> GetCartByCustomerIdAsync(
        long userId,
        NpgsqlConnection? con = null,
        NpgsqlTransaction? transaction = null,
        CancellationToken token = default
    )
    {
        throw new NotImplementedException();
    }

    public Task<List<CartItem>> GetAllItemsFromCartByCustomerId(
        long userId,
        CancellationToken token
    )
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddItem(
        CartItem entity,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    )
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddItemToCartAsync(long userId, CartItem item)
    {
        throw new NotImplementedException();
    }

    public Task<ShoppingCart?> GetCartById(long id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<CartItem?> GetItemById(long id, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateItems(ShoppingCart cart)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateItems(CartItem items)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCartAsync(ShoppingCart cart)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteItemAsync(long cartItemId, long userId)
    {
        throw new NotImplementedException();
    }
}
