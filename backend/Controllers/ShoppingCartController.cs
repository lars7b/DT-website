using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    // this has to be extra secure
    [HttpGet]
    public async Task<ActionResult<ShoppingCart>> GetShoppingCart()
    {
        // should be one to one relationship so could be found with user id 
        var cart = await _shoppingCartService.GetShoppingCartByUserIdAsync(0); //should send user id
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCart(ShoppingCart cart)
    {
        bool result = await _shoppingCartService.CreateCartAsync(cart);
        if (result)
        {
            return CreatedAtAction(nameof(GetShoppingCart), new { id = cart.Id }, cart);
        }
        return BadRequest();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateShoppingCart(long id, ShoppingCart cart)
    {
        if (id != cart.Id)
        {
            return BadRequest();
        }
        await _shoppingCartService.UpdateCartAsync(cart);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCart(long id)
    {
        await _shoppingCartService.DeleteCartAsync(id);
        return NoContent();
    }
}
