using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShoppingCartDto>> GetShoppingCart(CancellationToken token)
    {
        // should be one to one relationship so could be found with user id
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        var cart = await _shoppingCartService.GetShoppingCartByUserIdAsync(long.Parse(userId),token);
        if (cart == null)
        {
            return NotFound();
        }
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult> AddItemToShoppingCart([FromBody] CartItemDto item)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        if(item== null||item.Quantity<1){return BadRequest();}
        // could update or create
        bool result = await _shoppingCartService.AddItemsAsync(long.Parse(userId), item);
        if (result)
        {
            return NoContent();
        }
        return BadRequest();
    }
    [HttpPut("items")]
    public async Task<ActionResult> UpdateItemToShoppingCart([FromBody] CartItemDto item)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        if(item== null||item.Quantity<1){return BadRequest();}
        bool result = await _shoppingCartService.UpdateItemsAsync(long.Parse(userId), item);
        if (result)
        {
            return NoContent();
        }
        return BadRequest();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteShoppingCart()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        bool success = await _shoppingCartService.DeleteCartAsync(long.Parse(userId));
        if (success)
        {
            return NoContent();
        }
        return BadRequest();
    }

    [HttpDelete("Items/{cartitemId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteShoppingCartItem(long cartitemId)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        bool succes = await _shoppingCartService.DeleteCartItemAsync(
            cartitemId,
            long.Parse(userId)
        );
        if (succes)
        {
            return NoContent();
        }
        return BadRequest(); // possibly not found or more checks
    }
}
