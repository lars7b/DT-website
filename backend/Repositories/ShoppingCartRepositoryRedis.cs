using System;
using System.Threading.Tasks;
using System.Text.Json;
using Backend.Models;
using Npgsql;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace Backend.Repositories;

public class ShoppingCartRepositoryRedis : IShoppingCartRepository
{
    private readonly string _connectionString;
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ShoppingCartRepositoryRedis(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("RedisDefaultConnection")
            ?? throw new InvalidOperationException("Redis DB Connection missing");

        _redis = ConnectionMultiplexer.Connect(_connectionString);
        _db = _redis.GetDatabase();
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
        var key = $"cart:{userId}";
        var val = _db.StringGet(key);
        if (val.IsNullOrEmpty) return Task.FromResult<ShoppingCart?>(null);
        try
        {
            var cart = JsonSerializer.Deserialize<ShoppingCart>(val, _jsonOptions);
            return Task.FromResult(cart);
        }
        catch
        {
            return Task.FromResult<ShoppingCart?>(null);
        }
    }

    public Task<List<CartItem>> GetAllItemsFromCartByCustomerId(
        long userId,
        CancellationToken token
    )
    {
        return GetCartByCustomerIdAsync(userId).ContinueWith(t =>
        {
            var cart = t.Result;
            return cart?.Items ?? new List<CartItem>();
        }, token);
    }

    public Task<bool> AddItem(
        CartItem entity,
        NpgsqlConnection? connection = null,
        NpgsqlTransaction? transaction = null
    )
    {
        // Try to find the cart by CartId mapping
        if (entity == null) return Task.FromResult(false);
        var cartIdKey = $"cart:id:{entity.CartId}";
        var cust = _db.StringGet(cartIdKey);
        if (cust.IsNullOrEmpty) return Task.FromResult(false);
        if (!long.TryParse(cust, out var customerId)) return Task.FromResult(false);
        return AddItemToCartAsync(customerId, entity);
    }

    public Task<bool> AddItemToCartAsync(long userId, CartItem item)
    {
        var key = $"cart:{userId}";
        var val = _db.StringGet(key);
        ShoppingCart cart;
        if (val.IsNullOrEmpty)
        {
            // create a new cart
            var newCartId = (long)_db.StringIncrement("cart:nextId");
            cart = new ShoppingCart { Id = newCartId, CustomerId = userId, Items = new List<CartItem>() };
            // store mapping cart:id:{cartId} -> userId
            _db.StringSet($"cart:id:{cart.Id}", userId.ToString());
        }
        else
        {
            cart = JsonSerializer.Deserialize<ShoppingCart>(val, _jsonOptions) ?? new ShoppingCart { CustomerId = userId };
        }

        // assign item id
        var nextItemId = (long)_db.StringIncrement($"cart:{userId}:nextItemId");
        item.Id = nextItemId;
        item.CartId = cart.Id;

        cart.Items.Add(item);

        var serialized = JsonSerializer.Serialize(cart, _jsonOptions);
        _db.StringSet(key, serialized);
        // map item id to customer for quick lookup
        _db.StringSet($"item:id:{item.Id}", userId.ToString());

        return Task.FromResult(true);
    }

    public Task<ShoppingCart?> GetCartById(long id, CancellationToken token = default)
    {
        var mapped = _db.StringGet($"cart:id:{id}");
        if (mapped.IsNullOrEmpty) return Task.FromResult<ShoppingCart?>(null);
        if (!long.TryParse(mapped, out var userId)) return Task.FromResult<ShoppingCart?>(null);
        return GetCartByCustomerIdAsync(userId, null, null, token);
    }

    public Task<CartItem?> GetItemById(long id, CancellationToken token = default)
    {
        var mapped = _db.StringGet($"item:id:{id}");
        if (mapped.IsNullOrEmpty) return Task.FromResult<CartItem?>(null);
        if (!long.TryParse(mapped, out var userId)) return Task.FromResult<CartItem?>(null);
        var cartTask = GetCartByCustomerIdAsync(userId);
        var cart = cartTask.Result;
        var item = cart?.Items.FirstOrDefault(i => i.Id == id);
        return Task.FromResult(item);
    }

    public Task<bool> UpdateItems(ShoppingCart cart)
    {
        if (cart == null) return Task.FromResult(false);
        var key = $"cart:{cart.CustomerId}";
        // ensure cart has id
        if (cart.Id == 0)
        {
            cart.Id = (long)_db.StringIncrement("cart:nextId");
            _db.StringSet($"cart:id:{cart.Id}", cart.CustomerId.ToString());
        }

        foreach (var it in cart.Items)
        {
            if (it.Id == 0)
            {
                it.Id = (long)_db.StringIncrement($"cart:{cart.CustomerId}:nextItemId");
            }
            _db.StringSet($"item:id:{it.Id}", cart.CustomerId.ToString());
        }

        var serialized = JsonSerializer.Serialize(cart, _jsonOptions);
        _db.StringSet(key, serialized);
        return Task.FromResult(true);
    }

    public Task<bool> UpdateItems(CartItem items)
    {
        if (items == null) return Task.FromResult(false);
        var mapped = _db.StringGet($"item:id:{items.Id}");
        if (mapped.IsNullOrEmpty) return Task.FromResult(false);
        if (!long.TryParse(mapped, out var userId)) return Task.FromResult(false);
        var cart = GetCartByCustomerIdAsync(userId).Result;
        if (cart == null) return Task.FromResult(false);
        var idx = cart.Items.FindIndex(i => i.Id == items.Id);
        if (idx >= 0)
        {
            cart.Items[idx] = items;
            var serialized = JsonSerializer.Serialize(cart, _jsonOptions);
            _db.StringSet($"cart:{userId}", serialized);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> DeleteCartAsync(ShoppingCart cart)
    {
        if (cart == null) return Task.FromResult(false);
        var key = $"cart:{cart.CustomerId}";
        // remove item mappings
        foreach (var it in cart.Items)
        {
            _db.KeyDelete($"item:id:{it.Id}");
        }
        // remove cart mapping
        _db.KeyDelete($"cart:id:{cart.Id}");
        _db.KeyDelete(key);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteItemAsync(long cartItemId, long userId)
    {
        var cart = GetCartByCustomerIdAsync(userId).Result;
        if (cart == null) return Task.FromResult(false);
        var removed = cart.Items.RemoveAll(i => i.Id == cartItemId) > 0;
        if (!removed) return Task.FromResult(false);
        var serialized = JsonSerializer.Serialize(cart, _jsonOptions);
        _db.StringSet($"cart:{userId}", serialized);
        _db.KeyDelete($"item:id:{cartItemId}");
        return Task.FromResult(true);
    }
}
