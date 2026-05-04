using System.Security.Claims;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetOrder(long id,CancellationToken token)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        OrderDto? order = await _orderService.GetOrderByIdAsync(id, long.Parse(userId),token);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAllOrders(CancellationToken token)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        List<OrderDto> orders = await _orderService.GetOrdersAsync(long.Parse(userId),token);
        // check length list
        return Ok(orders);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CreateOrder()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        bool success = await _orderService.CreateOrderAsync(long.Parse(userId)); //create order from cart
        if (success)
        {
            return NoContent();
        }
        return BadRequest();
    }

    [Authorize(Roles="Admin")]
    [HttpPost("{orderid:long}/items")] // can be put
    public async Task<ActionResult> AddItemToOrderAsync(long orderid,[FromBody] OrderItemDto item)
    {
        // check if order is already paid for if yes return badrequest
        // also check if item is already in order if yes update quantity instead of adding new item or make separate endpoint for updating quantity
        throw new NotImplementedException();
    }

    [Authorize(Roles="Admin")]
    [HttpDelete("items/{id:long}")] // can be put
    public async Task<ActionResult> RemoveItemFromOrderAsync(long id)
    {
        // check if order is already paid for if yes return badrequest
        throw new NotImplementedException();
    }

    [Authorize(Roles="Admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult> UpdateOrder(long id, [FromBody] OrderDto order)
    {
        throw new NotImplementedException();
        // // get user id and use
        // long userId=1;
        // await _orderService.UpdateOrderAsync(order,userId);
        // return NoContent();
    }

    [Authorize]
    [HttpPut("{id:long}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelOrder(long id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        bool succes = await _orderService.CancelOrderAsync(id, long.Parse(userId));
        if (succes == true)
        {
            return Ok();
        }
        return BadRequest();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteOrder(long id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        bool succes = await _orderService.DeleteOrderAsync(id, long.Parse(userId));
        if (succes == true)
        {
            return NoContent();
        }
        return BadRequest();
    }
}
