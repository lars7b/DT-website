using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> GetPaymentById(long id,CancellationToken cancellationToken)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }
        return Ok(payment);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments(CancellationToken cancellationToken)
    {
        // check if admin return all if not return only users payments
        var payments = await _paymentService.GetAllPaymentsAsync(cancellationToken);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult> CreatePayment(Payment payment) // of order
    {
        bool result = await _paymentService.CreatePaymentAsync(payment);
        if (result)
        {
            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, payment);
        }
        return BadRequest();
    }

    [Authorize(Roles="Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePayment(long id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }
        bool succesful = await _paymentService.UpdatePaymentAsync(payment);
        if (succesful == false)
        {
            return BadRequest("Updating was unsuccesful");
        }
        return NoContent();
    }

    [Authorize(Roles="Admin")]
    [HttpDelete("{id}")]
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
