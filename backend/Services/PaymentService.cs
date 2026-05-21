using Backend.Models;
using Backend.Repositories;
using Backend.DTOs;
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

    public async Task<Payment?> GetPaymentByIdAsync(
        long id,
        long userid,
        CancellationToken cancellationToken = default
    )
    {
        Payment? payment = await _repository.GetById(id, userid, cancellationToken);
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(
        long userid,
        CancellationToken cancellationToken = default
    )
    {
        return await _repository.GetAll(userid, cancellationToken);
    }

    public async Task<bool> CreatePaymentAsync(long userid, CreatePaymentDto paymentdto)
    {
        // TODO userid check
        // needs to check if order id exists and check price before create
        Payment payment = new Payment
        {
            Amount = await _repository.GetAmountForOrder(paymentdto.OrderId),
            PaymentDate = DateTime.Now,
            OrderId = paymentdto.OrderId,
            PaymentMethod = paymentdto.PaymentMethod,
            Status = paymentdto.Status,
        };
        bool result = await _repository.Add(payment,userid);
        return result;
    }

    /// <summary>
    /// updates payment if exists
    /// </summary>
    /// <param name="payment">the payment you want to update</param>
    /// /// <param name="cancellationToken">cancellation token for the get method</param>
    /// <returns></returns>
    public async Task<bool> UpdatePaymentAsync(
        UpdatePaymentDto payment,
        CancellationToken cancellationToken = default
    )
    {
        Payment? existingPayment = await _repository.GetById(payment.Id, null, cancellationToken);
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
        return await _repository.Delete(id);
    }
}
