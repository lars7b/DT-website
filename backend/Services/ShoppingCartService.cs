namespace Backend.Services;

using Backend.Models;
using Backend.Repositories;

public sealed class ShoppingCartService : IShoppingCartService
{
    private readonly ShoppingCartRepository _shoppingCartRepository;
    private readonly CartItemRepository _cartItemRepository;

    public ShoppingCartService(
        ShoppingCartRepository shoppingCartRepository,
        CartItemRepository cartItemRepository
    )
    {
        _shoppingCartRepository = shoppingCartRepository;
        _cartItemRepository = cartItemRepository;
    }

    public async Task<ShoppingCart?> GetShoppingCartByUserIdAsync(long userId)
    {
        // should be one to one relationship so could be found with user id
        return await _shoppingCartRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> CreateCartAsync(ShoppingCart cart)
    {
        return await _shoppingCartRepository.Add(cart);
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

    public async Task<bool> DeleteCartAsync(long id)
    {
        ShoppingCart? cart = await _shoppingCartRepository.GetByUserIdAsync(id);
        if (cart != null)
        {
            return await _shoppingCartRepository.Delete(cart);
        }
        return false;
    }
}
