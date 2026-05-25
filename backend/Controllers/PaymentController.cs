using System.Security.Claims;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Payment>> GetPaymentById(
        long id,
        CancellationToken cancellationToken
    )
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) //TODO check if id is long
        {
            return Unauthorized();
        }
        Payment? payment = await _paymentService.GetPaymentByIdAsync(id,long.Parse(userId), cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }

    [HttpGet] //TODO add query for admin per user or ...
    public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments(
        CancellationToken cancellationToken
    )
    {
        // check if admin return all if not return only users payments
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userrole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null || userrole == null)
        {
            return Unauthorized();
        }
        var payments = await _paymentService.GetAllPaymentsAsync(long.Parse(userId),cancellationToken);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult> CreatePayment([FromBody] Payment payment) // of order
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? userrole = User.FindFirst(ClaimTypes.Role)?.Value;
        if (userId == null || userrole == null)
        {
            return Unauthorized();
        }
        bool result = await _paymentService.CreatePaymentAsync(long.Parse(userId),payment);
        if (result)
        {
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
        }
        return BadRequest();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdatePayment(
        long id,
        [FromBody] Payment payment,
        CancellationToken cancellationToken
    )
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }
        bool succesful = await _paymentService.UpdatePaymentAsync(payment, cancellationToken);
        if (succesful == false)
        {
            return BadRequest("Updating was unsuccesful");
        }
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
    public async Task<ActionResult> DeletePayment(long id)
    {
        bool succesful = await _paymentService.DeletePaymentAsync(id);
        if (succesful == false)
        {
            return BadRequest("Deleting was unsuccesful");
        }
        return NoContent();
    }
}
