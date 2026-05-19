using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }


    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetMyProfile()
    {
        int userId = GetUserIdFromToken();
        var customerDto = await _customerService.GetCustomerAsync(userId);

        if (customerDto == null) return NotFound(new { message = "Profiel niet gevonden." });
        return Ok(customerDto);

    }

    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] CustomerDto request)
    {
        int userId = GetUserIdFromToken();
        var result = await _customerService.UpdateCustomerAsync(userId, request);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(new { message = result.Message });
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile([FromBody] string password)
    {
        int userId = GetUserIdFromToken();
        var result = await _customerService.DeleteCustomerAsync(userId, password);

        if (!result.Success)
        {
            return NotFound(new { message = result.Message });
        }
        return Ok(new { message = result.Message });      
    }


    /* 
    Eventuele andere endpoints

    Wishlist
    GET api/customer/me/wishlist
    Haalt een lijst op van alle opgeslagen producten (vaak geretourneerd als een lijst van ProductDto of WishlistItemDto).
    POST api/customer/me/wishlist/{productId}
    Voegt een specifiek product toe aan de verlanglijst van de klant.
    DELETE api/customer/me/wishlist/{productId}
    Verwijdert een specifiek product van de verlanglijst.

    Adressenboek
    GET api/customer/me/addresses
    Haalt alle geregistreerde adressen van de klant op.
    POST api/customer/me/addresses
    Voegt een nieuw aflever- of factuuradres toe.
    PUT api/customer/me/addresses/{addressId}
    Wijzigt een specifiek bestaand adres.
    DELETE api/customer/me/addresses/{addressId}
    Verwijdert een specifiek adres uit het profiel.
    
    */

}
