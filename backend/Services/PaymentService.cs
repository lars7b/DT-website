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

    public async Task<bool> CreatePaymentAsync(long userid, Payment payment)
    {
        // TODO userid check
        // needs to check if order id exists and check price before create
        bool result = await _repository.Add(payment);
        return await Task.FromResult(result);
    }

    public async Task<bool> CreatePaymentAsync(long userid, long orderId, string method)
    {
        Payment payment = new Payment
        {
            Amount = await _repository.GetAmountForOrder(orderId),
            PaymentDate = DateTime.Now,
            OrderId = orderId,
            PaymentMethod = method,
            Status = "Paid",
        };
        return await CreatePaymentAsync(userid, payment);
    }

    public async Task<bool> UpdatePaymentAsync(
        Payment payment,
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
            return await _repository.Update(existingPayment);
        }
        return false;
    }

    public async Task<bool> DeletePaymentAsync(
        long id,
        CancellationToken cancellationToken = default
    )
    {
        Payment? payment = await _repository.GetById(id, null, cancellationToken);
        if (payment != null)
        {
            return await _repository.Delete(payment);
        }
        return false;
    }
}
