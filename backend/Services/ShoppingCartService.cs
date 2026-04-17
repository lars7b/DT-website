namespace Backend.Services;

using Backend.Models;
using Backend.Repositories;

public sealed class ShoppingCartService //: IShoppingCartService
{
    private readonly ShoppingCartRepository _shoppingCartRepository;

    public ShoppingCartService(ShoppingCartRepository shoppingCartRepository)
    {
        _shoppingCartRepository = shoppingCartRepository;
    }

    public async Task<ShoppingCart?> GetShoppingCartByUserIdAsync(long userId)
    {
        // should be one to one relationship so could be found with user id
        //should return all items
        return await _shoppingCartRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> AddItemsAsync(long userid, CartItem items)
    {
        if (items.CartId == null)
        {
            _shoppingCartRepository.CreateCart(new ShoppingCart { }); //
        }
        //get cart and check if user same

        return await _shoppingCartRepository.AddItem();
    }

    public async Task<bool> UpdateCartAsync(ShoppingCart cart)
    {
        ShoppingCart? existingCart = await _shoppingCartRepository.GetById(cart.Id);
        if (existingCart != null)
        {
            existingCart.CustomerId = cart.CustomerId;
            return await _shoppingCartRepository.Update(existingCart);
        }
        return false;
    }

    public async Task<bool> DeleteCartAsync(long userid)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetByUserIdAsync(userid);
        if (cart != null)
        {
            return await _shoppingCartRepository.Delete(cart);
        }
        return false;
    }
}
