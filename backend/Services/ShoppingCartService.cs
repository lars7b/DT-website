namespace Backend.Services;

using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

public sealed class ShoppingCartService : IShoppingCartService
{
    // TODO implement IShoppingCartRepository
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId, CancellationToken token =default)
    {
        // should be one to one relationship so could be found with user id
        List<CartItem> cartItems = await _shoppingCartRepository.GetAllItemsFromCartByCustomerId(
            userId,token
        );
        // TODO check role
        if (cartItems == null || cartItems.Count < 1)
        {
            return null;
        }
         // check why id is 0 when returned 
        var cartItemsDtos = cartItems
            .Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
            })
            .ToList();

        return new ShoppingCartDto
        {
            Id = cartItems.First().CartId,
            CustomerId = userId,
            Items = cartItemsDtos,
        };
    }

    public async Task<bool> AddItemsAsync(long userid, CartItemDto items)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userid);
        if (cart == null || cart.Id == null)
        {
            await _shoppingCartRepository.CreateCart(new ShoppingCart { CustomerId = userid });
            cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userid);
        }
        // TODO check if product exists
        // check if items exists and if there is quantity change
        CartItem cartItem = new CartItem
        {
            ProductId = items.ProductId,
            Quantity = items.Quantity,
            CartId = cart!.Id,
        };
        return await _shoppingCartRepository.AddItem(cartItem);
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
