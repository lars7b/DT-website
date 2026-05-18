using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderHistoryService _orderHistoryService;

    public OrdersController(IOrderHistoryService orderHistoryService)
    {
        _orderHistoryService = orderHistoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderHistory>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<OrderHistory>>> GetOrderHistory(
        int customerId,
        CancellationToken cancellationToken)
    {
        if (!await _orderHistoryService.CustomerExistsAsync(customerId, cancellationToken))
        {
            return NotFound();
        }

        var orders = await _orderHistoryService.GetOrderHistoryAsync(customerId, cancellationToken);
        return Ok(orders);
    }
}
