using System.Security.Claims;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/customers/me/favorites")]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private int GetCustomerIdFromToken()
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(customerIdClaim!.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FavoriteProduct>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<FavoriteProduct>>> GetFavorites(
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerIdFromToken();

        if (!await _favoriteService.CustomerExistsAsync(customerId, cancellationToken))
        {
            return NotFound();
        }

        var favorites = await _favoriteService.GetFavoritesByCustomerAsync(customerId, cancellationToken);
        return Ok(favorites);
    }

    [HttpPost("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddFavorite(
        int productId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerIdFromToken();

        if (!await _favoriteService.CustomerExistsAsync(customerId, cancellationToken) ||
            !await _favoriteService.ProductExistsAsync(productId, cancellationToken))
        {
            return NotFound();
        }

        var added = await _favoriteService.AddFavoriteAsync(customerId, productId, cancellationToken);
        if (!added)
        {
            return Conflict(new { message = "This product is already saved as a favorite." });
        }

        return CreatedAtAction(nameof(GetFavorites), null, null);
    }

    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(
        int productId,
        CancellationToken cancellationToken)
    {
        var customerId = GetCustomerIdFromToken();

        if (!await _favoriteService.CustomerExistsAsync(customerId, cancellationToken))
        {
            return NotFound();
        }

        var removed = await _favoriteService.RemoveFavoriteAsync(customerId, productId, cancellationToken);
        if (!removed)
        {
            return NotFound();
        }

        return NoContent();
    }
}
