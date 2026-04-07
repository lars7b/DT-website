using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IRepository<Payment> _repository;

    public PaymentService(IRepository<Payment> repository)
    {
        _repository = repository;
    }

    public async Task<Payment?> GetPaymentByIdAsync(long id)
    {
        Payment? payment = await _repository.GetById(id);
        return payment;
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
    {
        return await _repository.GetAll();
    }

    public async Task<bool> CreatePaymentAsync(Payment payment)
    {
        bool result = await _repository.Add(payment);
        return await Task.FromResult(result);
    }

    public async Task<bool> CreatePaymentAsync(long orderId, string method)
    {
        Payment payment = new Payment
        {
            Amount = 0, //GetAmountForOrder(orderId), //
            PaymentDate = DateTime.Now,
            OrderId = orderId,
            PaymentMethod = method,
        };
        return await CreatePaymentAsync(payment);
    }

    public async Task<bool> UpdatePaymentAsync(Payment payment)
    {
        Payment? existingPayment = await _repository.GetById(payment.Id);
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

    public async Task<bool> DeletePaymentAsync(long id)
    {
        Payment? payment = await _repository.GetById(id);
        if (payment != null)
        {
            return await _repository.Delete(payment);
        }
        return false;
    }
}
