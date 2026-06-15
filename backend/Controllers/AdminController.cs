using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;
using Backend.DTOs;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    public AdminController(IEmployeeService employeeService, ICustomerService customerService, IOrderService orderService)
    {
        _employeeService = employeeService;
        _customerService = customerService;
        _orderService = orderService;
    }

    [HttpPost("employees")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto request)
    {
        var result = await _employeeService.CreateEmployeeAsync(request);
        
        if (!result.Success) return BadRequest(new { message = result.Message });
        
        return Ok(new { message = result.Message });
    }

    [HttpPut("orders/{id:long}")]
    public async Task<ActionResult> UpdateOrderAsAdmin(long id, [FromBody] OrderDto order)
    {
        var success = await _orderService.UpdateOrderAsync(order, id);
        if (!success) return NotFound();
        
        return NoContent();
    }
}