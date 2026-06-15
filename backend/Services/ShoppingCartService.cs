namespace Backend.Services;

using System.Text.Json;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using StackExchange.Redis;

public sealed class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IDatabase _redis;

    public ShoppingCartService(
        IShoppingCartRepository shoppingCartRepository,
        IConnectionMultiplexer redis
    )
    {
        _shoppingCartRepository = shoppingCartRepository;
        _redis = redis.GetDatabase();
    }

    /// <summary>
    /// returns shopping cart for the given user id
    /// should be one to one relationship so could be found with user id
    /// redis uses user id as key and shopping cart as value
    /// </summary>
    /// <param name="userId">the user id of the customer that has that shopping cart</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(
        long userId,
        CancellationToken token = default
    )
    {
        string cacheKey = $"shopping_cart:{userId}"; // change to customer id
        var cachedData = await _redis.StringGetAsync(cacheKey);

        if (!cachedData.IsNullOrEmpty)
        {
            try
            {
                return JsonSerializer.Deserialize<ShoppingCartDto>(cachedData!);
            }
            catch
            {
                await _redis.KeyDeleteAsync(cacheKey);
            }
        }
        List<CartItem> cartItems = await _shoppingCartRepository.GetAllItemsFromCartByCustomerId(
            userId,
            token
        );
        if (cartItems == null || cartItems.Count < 1)
        {
            return null;
        }
        List<CartItemDto> cartItemsDtos = cartItems
            .Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                ProductName = item.Product == null
                    ? null
                    : item.Product.Name,
                ProductDescription = item.Product == null? null : item.Product.Description,
                PricePerUnit = item.Product == null? null : item.Product.Price,
            })
            .ToList();
        ShoppingCartDto shoppingcart = new ShoppingCartDto
        {
            Id = cartItems.First().CartId,
            CustomerId = userId,
            /// TODO use custmerid not userid (possibly return whole shoppingcart with items via repo)
            Items = cartItemsDtos,
        };
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(shoppingcart),
            TimeSpan.FromMinutes(15)
        ); // how long it holds it
        return shoppingcart;
    }

    public async Task<bool> AddItemsAsync(long userid, CartItemDto items)
    {
        if (items.Quantity < 1||items.ProductId<1)
        {
            return false;
        }
        CartItem cartItem = new CartItem
        {
            ProductId = items.ProductId,
            Quantity = items.Quantity,
            CartId = items.Id,
        };
        bool success = await _shoppingCartRepository.AddItemToCartAsync(userid, cartItem);
        if (success)
        {
            await _redis.KeyDeleteAsync($"shopping_cart:{userid}");
        }
        return success;
    }

    public async Task<bool> UpdateItemsAsync(long userId, CartItemDto item)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByUserIdAsync(userId);
        if (cart == null || cart.Id == null)
        {
            return false;
        }
        CartItem cartItem = new CartItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            CartId = cart.Id,
        };
        bool success = await _shoppingCartRepository.UpdateItems(cartItem);
        if (success)
        {
            await _redis.KeyDeleteAsync($"shopping_cart:{userId}");
        }
        return success;
    }

    public async Task<bool> UpdateCartAsync(ShoppingCartDto cart)
    {
        ShoppingCart? existingCart = await _shoppingCartRepository.GetCartByUserIdAsync(
            cart.CustomerId
        );
        if (existingCart != null)
        {
            existingCart.CustomerId = cart.CustomerId;
            existingCart.Items = cart
                .Items.Select(item => new CartItem
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    CartId = cart.Id,
                })
                .ToList();
            bool success = await _shoppingCartRepository.UpdateItems(existingCart);
            if (success)
            {
                await _redis.KeyDeleteAsync($"shopping_cart:{existingCart.CustomerId}"); // existingCart.CustomerId 
            }
            return success;
        }
        return false;
    }

    public async Task<bool> DeleteCartAsync(long userid)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByUserIdAsync(userid);
        if (cart != null)
        {
            bool success = await _shoppingCartRepository.DeleteCartAsync(cart); // can delete via userid (and shorten the process)
            if (success)
            {
                await _redis.KeyDeleteAsync($"shopping_cart:{userid}");
            }
            return success;
        }
        return false;
    }

    public async Task<bool> DeleteCartItemAsync(long cartItemId, long userId)
    {
        bool success = await _shoppingCartRepository.DeleteItemAsync(cartItemId, userId);
        if (success)
        {
            await _redis.KeyDeleteAsync($"shopping_cart:{userId}");
        }
        return success;
    }
}