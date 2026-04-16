using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;

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

    [HttpGet("customers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Customer>>> GetAllCustomers()
    {
        var customers = await _employeeService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("customers/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Customer>> GetCustomer(int id)
    {
        return default;
    }

    [HttpGet("customers/{id}/orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Order>>> GetCustomerOrders(int id)
    {
        return default;
    }

    [HttpDelete("customers/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> DeleteCustomer(int id)
    {
        return default;
    }

}