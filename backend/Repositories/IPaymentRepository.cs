using Backend.Models;

namespace Backend.Repositories;

public interface IPaymentRepository
{
    public Task<List<Payment>> GetAll(CancellationToken cancellationToken);
    public Task<Payment?> GetById(long id,CancellationToken cancellationToken);
    public Task<bool> Add(Payment payment);
    public Task<bool> Update(Payment payment);
    public Task<bool> Delete(Payment? payment);

    public Task<List<Payment>> GetByOrderId(long orderId);
    public Task<List<Payment>> GetByUser(long userId);

    public Task<decimal> GetAmountForOrder(long orderId);
}