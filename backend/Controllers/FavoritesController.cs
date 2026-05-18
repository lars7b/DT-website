using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/favorites")]
public sealed class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FavoriteProduct>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<FavoriteProduct>>> GetFavorites(
        int customerId,
        CancellationToken cancellationToken)
    {
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
        int customerId,
        int productId,
        CancellationToken cancellationToken)
    {
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

        return CreatedAtAction(nameof(GetFavorites), new { customerId }, null);
    }

    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(
        int customerId,
        int productId,
        CancellationToken cancellationToken)
    {
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