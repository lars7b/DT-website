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
        // TODO check role
        if (cartItems == null || cartItems.Count < 1)
        {
            return null;
        }
        // check why id is 0 when returned
        List<CartItemDto> cartItemsDtos = cartItems
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
            CustomerId = userId, ///
            Items = cartItemsDtos,
        };
    }
 
    public async Task<bool> AddItemsAsync(long userid, CartItemDto items)
    {
        // ShoppingCart? cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userid);
        // if (cart == null || cart.Id == null)
        // {
        //     await _shoppingCartRepository.CreateCart(new ShoppingCart { CustomerId = userid });
        //     cart = await _shoppingCartRepository.GetCartByCustomerIdAsync(userid); // get card from create method
        // }
        // // // TODO check if product exists + check if quantity is valid + fix logic
        // // for(int i=0;i<cart.Items.Count;i++)
        // // {
        // //     if(cart.Items[i].ProductId == items.ProductId){
        // //         cart.Items[i].Quantity = items.Quantity; //could be +=
        // //         return await _shoppingCartRepository.UpdateItems(cart.Items[i]);
        // //     }
        // // }
        // CartItem cartItem = new CartItem
        // {
        //     ProductId = items.ProductId,
        //     Quantity = items.Quantity,
        //     CartId = cart!.Id,
        // };
        // return await _shoppingCartRepository.AddItem(cartItem);


        CartItem cartItem = new CartItem
        {
            ProductId = items.ProductId,
            Quantity = items.Quantity,
            CartId = items.Id,
        };
        return await _shoppingCartRepository.AddItemToCartAsync(userid,cartItem);
        // throw new NotImplementedException();
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
