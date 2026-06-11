using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Employee,Admin")]

public class EmployeeController : ControllerBase
{

    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    private int GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return int.Parse(userIdClaim!.Value);
    }
    
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetMyProfile()
    {
        int employeeId = GetUserIdFromToken();
        var profile = await _employeeService.GetEmployeeProfileAsync(employeeId);

        if (profile == null) return NotFound(new { message = "Medewerkersprofiel niet gevonden." });
        return profile;
    }

    /*
    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] EmployeeDto request)
    {
        int employeeId = GetUserIdFromToken();
        var result = await _employeeService.UpdateEmployeeAsync(employeeId, request);

        if (!result.Success) return NotFound(new { message = result.Message });
        
        return Ok(new { message = result.Message });
    }
    */

    [HttpGet("customer-by-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerAdminDto>> GetCustomerByEmail([FromQuery] string email)
    {
        var customer = await _employeeService.GetCustomerByEmailAsync(email);
        if (customer == null) 
        {
            return NotFound(new { message = "Geen klant gevonden met dit e-mailadres." });
        }
        return Ok(customer);
    }

    [HttpGet("customers/{userId}/orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(int userId)
    {
        var orders = await _employeeService.GetOrdersByUserIdAsync(userId);
        return Ok(orders);
    }
    
    /*

    Andere mogelijke endpoints voor medewerkers

    [HttpGet("customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CustomerDto>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return customers;
    }

    [HttpGet("customers/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _customerService.GetCustomerProfileAsync(id);
        
        if (customer == null) return NotFound(new { message = "Klant niet gevonden." });
        return customer;
    }

    [HttpDelete("customers/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var result = await _customerService.DeleteCustomerAccountAsync(id);

        if (!result.Success) return NotFound(new { message = result.Message });
        
        return Ok(new { message = "Klant succesvol verwijderd/geanonimiseerd." });
    }

    [HttpGet("customers/{id}/orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetCustomerOrders(int id)
    {
        var orders = await _orderService.GetOrdersForUserAsync(id);
        return orders;
    }

    [HttpGet("orders/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(int id)
    {
        var order = await _orderService.GetOrderDetailsAsync(id);
        
        if (order == null) return NotFound(new { message = "Order niet gevonden." });
        return order;
    }

    [HttpPatch("orders/{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto request)
    {
        var result = await _orderService.UpdateOrderStatusAsync(id, request.NewStatus);

        if (!result.Success) return BadRequest(new { message = result.Message });
        
        return Ok(new { message = "Order status succesvol bijgewerkt." });
    }

    [HttpPost("products")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto request)
    {
        var result = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetOrderDetails), new { id = result.ProductId }, new { message = "Product aangemaakt." });
    }

    [HttpPatch("products/{id}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto request)
    {
        var result = await _productService.UpdateStockAsync(id, request.QuantityAdded);

        if (!result.Success) return BadRequest(new { message = result.Message });
        
        return Ok(new { message = "Voorraad succesvol bijgewerkt." });
    }
    */
}