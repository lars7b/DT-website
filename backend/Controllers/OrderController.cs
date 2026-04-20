using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using Backend.Models;
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
    [HttpGet("{id}")]
    public async Task<ActionResult> GetOrder(long id)
    {
        //get user id and use
        await _orderService.GetOrderByIdAsync(id,1);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet]
    public async Task<ActionResult> GetAllOrders()
    {
        // get user id and use
        long userId=1;
        await _orderService.GetOrdersAsync(userId);
        return NoContent();
    }

}