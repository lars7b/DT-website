namespace Backend.Services;

using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

public sealed class ShoppingCartService : IShoppingCartService
{
    private readonly IShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<ShoppingCartDto?> GetShoppingCartByUserIdAsync(long userId, CancellationToken token =default)
    {
        // should be one to one relationship so could be found with user id
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
                Product = item.Product == null
                    ? null
                    : new ProductDto
                    {
                        Id = item.Product.Id,
                        Name = item.Product.Name,
                        Description = item.Product.Description,
                        Price = item.Product.Price,
                        CategoryName = item.Product.Category?.Name,
                        SubcategoryName = item.Product.Subcategory?.Name,
                    },
            })
            .ToList();

        return new ShoppingCartDto
        {
            Id = cartItems.First().CartId,
            CustomerId = userId, ///
            Items = cartItemsDtos,
        };
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
