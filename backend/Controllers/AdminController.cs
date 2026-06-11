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

    public AdminController(IEmployeeService employeeService, ICustomerService customerService)
    {
        _employeeService = employeeService;
        _customerService = customerService;
    }

    [HttpPost("employees")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto request)
    {
        var result = await _employeeService.CreateEmployeeAsync(request);
        
        if (!result.Success) return BadRequest(new { message = result.Message });
        
        return Ok(new { message = result.Message });
    }
}