namespace Backend.Services;

using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using StackExchange.Redis;
using System.Text.Json;

public sealed class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;
    private readonly IDatabase _redis;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, IDatabase redis)
    {
        _shoppingCartRepository = shoppingCartRepository;
        _redis = redis;
    }

    /// <summary>
    /// returns shopping cart for the given user id
    /// should be one to one relationship so could be found with user id
    /// redis uses user id as key and shopping cart as value
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId, CancellationToken token =default)
    {
        string cacheKey = $"shopping_cart:{userId}";
        var cachedData = await _redis.StringGetAsync(cacheKey);

        if (!cachedData.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<ShoppingCartDto>(cachedData!);
        }
        List<CartItem> cartItems = await _shoppingCartRepository.GetAllItemsFromCartByCustomerId(userId,token);
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
            })
            .ToList();
        ShoppingCartDto shoppingcart = new ShoppingCartDto
        {
            Id = cartItems.First().CartId,
            CustomerId = userId, /// TODO use custmerid not userid (possibly return whole shoppingcart with items via repo)
            Items = cartItemsDtos,
        };
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(shoppingcart),
            TimeSpan.FromMinutes(15)); // how long it holds it 
        return shoppingcart;
    }
 
    public async Task<bool> AddItemsAsync(long userid, CartItemDto items)
    {
        if (items.Quantity<1){
            return false;
        }
        CartItem cartItem = new CartItem
        {
            ProductId = items.ProductId,
            Quantity = items.Quantity,
            CartId = items.Id,
        };
        return await _shoppingCartRepository.AddItemToCartAsync(userid,cartItem);
    }

    public async Task<bool> UpdateItemsAsync(long userId, CartItemDto item)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userId);
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
        return await _shoppingCartRepository.UpdateItems(cartItem);
    }

    public async Task<bool> UpdateCartAsync(ShoppingCartDto cart)
    {
        ShoppingCart? existingCart = await _shoppingCartRepository.GetCartByCustomerIdAsync(
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
            return await _shoppingCartRepository.UpdateItems(existingCart);
        }
        return false;
    }

    public async Task<bool> DeleteCartAsync(long userid)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userid);
        if (cart != null)
        {
            return await _shoppingCartRepository.DeleteCartAsync(cart); // can delete via userid (and shorten the process)
        }
        return false;
    }
    public async Task<bool> DeleteCartItemAsync(long cartItemId, long userId)
    {
        return await _shoppingCartRepository.DeleteItemAsync(cartItemId, userId);
    }
}
