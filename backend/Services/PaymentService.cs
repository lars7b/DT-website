using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Isopoh.Cryptography.Argon2;

// TODO https://www.techrepublic.com/article/online-payment-security/
namespace Backend.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;

    public PaymentService(IPaymentRepository repository)
    {
        _repository = repository;
    }

    // use encryption

    public async Task<PaymentDto?> GetPaymentByIdAsync(
        long id,
        long userid,
        CancellationToken cancellationToken = default
    )
    {
        Payment? payment = await _repository.GetById(id, userid, cancellationToken);
        if (payment == null)
        {
            return null;
        }
        PaymentDto paymentDto = new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            Status = payment.Status,
            OrderId = payment.OrderId,
        };
        return paymentDto;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(
        long userid,
        CancellationToken cancellationToken = default
    )
    {
        IEnumerable<Payment> payments = await _repository.GetAll(userid, cancellationToken);
        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            OrderId = p.OrderId,
        });
    }

    public async Task<bool> CreatePaymentAsync(long userid, CreatePaymentDto paymentdto)
    {
        // needs to check if order id exists and check price before create
        long? orderid =
            await _repository.GetPendingOrderIdForUser(userid)
            ?? throw new Exception("No pending order found");
        ;
        Payment payment = new Payment
        {
            Amount = 0, // will be updated in repo with the correct amount using the order
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = paymentdto.PaymentMethod, 
            OrderId =
                paymentdto.OrderId
                ?? (orderid ?? throw new Exception("No pending order found for user")),
            Status = "Pending"//paymentdto.Status ?? "Pending",
        };
        // if (payment.OrderId != orderid)
        // {
        //     return false;
        // }

        Payment result = await _repository.Add(payment); // mogelijk een overload alleen met alleen userid die pending ophaalt
        if (
            result == null
            || result.Id == 0
            // || result.OrderId != orderid
            || result.Amount != await _repository.GetAmountForOrder(payment.OrderId)
            || result.Amount <= 0
        )
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// updates payment if exists
    /// </summary>
    /// <param name="payment">the payment you want to update</param>
    /// <param name="cancellationToken">cancellation token for the get method</param>
    /// <returns></returns>
    public async Task<bool> UpdatePaymentAsync(
        PaymentDto payment,
        CancellationToken cancellationToken = default
    )
    {
        Payment? existingPayment = await _repository.GetById(
            id: payment.Id,
            userid: null,
            cancellationToken: cancellationToken
        );
        if (existingPayment != null)
        {
            existingPayment.Amount = payment.Amount;
            existingPayment.PaymentDate = payment.PaymentDate;
            existingPayment.PaymentMethod = payment.PaymentMethod;
            existingPayment.OrderId = payment.OrderId;
            existingPayment.Status = payment.Status;
            return await _repository.Update(existingPayment);
        }
        return false;
    }

    public async Task<bool> DeletePaymentAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _repository.Delete(id, cancellationToken);
    }
}
