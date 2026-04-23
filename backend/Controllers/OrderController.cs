using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using Backend.Models;
using Backend.DTOs;
using System.Security.Claims;
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
    public async Task<ActionResult> GetOrder(long id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null){return Unauthorized();}
        Order? order = await _orderService.GetOrderByIdAsync(id, long.Parse(userId));
        if (order == null){return NotFound();}
        return Ok(order);
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetAllOrders()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null){return Unauthorized();}
        List<Order> orders = await _orderService.GetOrdersAsync(long.Parse(userId));
        // check length list
        return Ok(orders);
    }
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateOrder()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null){return Unauthorized();}
        bool succes = await _orderService.CreateOrderAsync(long.Parse(userId)); //create order from cart
        if (succes){
            return Ok();
        }return BadRequest();
    }
    [Authorize]
    [HttpPost("add-item")] // can possibly change id/add-item and also be put
    public async Task<ActionResult> AddItemToOrderAsync([FromBody] OrderItem item)
    {
        // check if order is already paid for if yes return badrequest
        // also check if item is already in order if yes update quantity instead of adding new item or make separate endpoint for updating quantity
        throw new NotImplementedException();
    }
    [Authorize]
    [HttpDelete("remove-item/{id:long}")] // can possibly change id/add-item and also be put
    public async Task<ActionResult> RemoveItemFromOrderAsync(long id)
    {
        // check if order is already paid for if yes return badrequest
        throw new NotImplementedException();
    }
    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<ActionResult> UpdateOrder(long id, [FromBody] Order order)
    {
        throw new NotImplementedException();
        // // get user id and use
        // long userId=1;
        // await _orderService.UpdateOrderAsync(order,userId);
        // return NoContent();
    }
    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeleteOrder(long id)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null){return Unauthorized();}
        await _orderService.DeleteOrderAsync(id,long.Parse(userId));
        throw new NotImplementedException();
    }

}