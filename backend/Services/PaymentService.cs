namespace Backend.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IRepository<Payment> _repository;

    public Task<Payment>? GetPaymentByIdAsync(long id)
    {
        Payment? payment = _repository.GetById(id);
        return Task.FromResult(payment);
    }

    public Task<IEnumerable<Payment>> GetAllPaymentsAsync()
    {
        return Task.FromResult(_repository.GetAll());
    }

    public Task<bool> CreatePaymentAsync(Payment payment)
    {
        bool result = _repository.Add(payment);
        return Task.FromResult(result);
    }

    public Task<bool> CreatePaymentAsync(long orderId, string method)
    {
        Payment payment = new Payment
        {
            Amount = GetAmountForOrder(orderId), //
            Date = DateTime.Now,
            OrderId = orderId,
            Method = method,
        };
        return CreatePaymentAsync(payment);
    }

    public Task UpdatePaymentAsync(Payment payment)
    {
        Payment? existingPayment = _repository.GetById(payment.Id);
        if (existingPayment != null)
        {
            existingPayment.Amount = payment.Amount;
            existingPayment.Date = payment.Date;
            existingPayment.Method = payment.Method;
            existingPayment.OrderId = payment.OrderId;
        }
        return Task.CompletedTask;
    }

    public Task DeletePaymentAsync(long id)
    {
        Payment? payment = _repository.GetById(id);
        if (payment != null)
        {
            _repository.Delete(payment);
        }
        return Task.CompletedTask;
    }
}
