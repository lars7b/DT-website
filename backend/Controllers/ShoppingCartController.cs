using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.DTOs;
namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpGet]
    public async Task<ActionResult<ShoppingCartDto>> GetShoppingCart(CancellationToken token)
    {
        // should be one to one relationship so could be found with user id
        var cart = await _shoppingCartService.GetShoppingCartByUserIdAsync(1); //should send user id
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    [HttpPost("Item")]
    public async Task<ActionResult> CreateCart(long productId,int quantity) // cartitem (Dto) or  long productId,int quantity
    {
        bool result = await _shoppingCartService.AddItemsAsync(1,new CartItemDto { ProductId = productId,Quantity = quantity}); //should be changed
        if (result)
        {
            return CreatedAtAction(nameof(GetShoppingCart), new { id = 1 }, null); //
        }
        return BadRequest();
    }

    [HttpPut("Item/{id}")]
    public async Task<ActionResult> UpdateShoppingCart(long id, ShoppingCartDto cart)
    {
        if (id != cart.Id)
        {
            return BadRequest();
        }
        await _shoppingCartService.UpdateCartAsync(cart);
        return NoContent();
    }

    [HttpDelete("Item/{id}")]
    public async Task<ActionResult> DeleteCartItem(long cartitemId)
    {
        await _shoppingCartService.DeleteCartAsync(cartitemId);
        return NoContent();
    }
}
