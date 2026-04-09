using Microsoft.AspNetCore.Mvc;
using Backend.DTOs;
using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;

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
        // helper method om ID uit JWT te halen.
        return 0;
    }


    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Customer>> GetMyProfile()
    {
        int userId = GetUserIdFromToken();


        return default;
    }

    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> UpdateProfile([FromBody] CustomerDto request)
    {
        int userId = GetUserIdFromToken();

        return default;
    }

    [HttpGet("me/orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Order>>> GetMyOrders()
    {
        int userId = GetUserIdFromToken();

        return default;

        //Bestel geschiedenis voor 1 klant.
        //JOIN tussen orders en order_items
    }
}
