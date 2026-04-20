namespace Backend.Services;

using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

public sealed class ShoppingCartService : IShoppingCartService
{
    private readonly ShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(ShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId)
    {
        // should be one to one relationship so could be found with user id
        //should return all items
        List<CartItem> cartItems = await _shoppingCartRepository.GetAllItemsFromCartByUserId(
            userId
        );
        if (cartItems == null || cartItems.Count == 0)
        {
            return null;
        }
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
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByUserIdAsync(userid);
        if (cart == null || cart.Id == null)
        {
            _shoppingCartRepository.CreateCart(new ShoppingCart { CustomerId = userid }); //
            cart = await _shoppingCartRepository.GetCartByUserIdAsync(userid);
        }
        CartItem cartItem = new CartItem
        {
            ProductId = items.ProductId,
            Quantity = items.Quantity,
            CartId = cart.Id,
        };
        return await _shoppingCartRepository.AddItem(cartItem);
    }

    public async Task<bool> UpdateCartAsync(ShoppingCartDto cart)
    {
        ShoppingCart? existingCart = await _shoppingCartRepository.GetCartByUserIdAsync(cart.Id);
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
        ShoppingCart? cart = await _shoppingCartRepository.GetCartByUserIdAsync(userid);
        if (cart != null)
        {
            return await _shoppingCartRepository.Delete(cart);
        }
        return false;
    }
}
