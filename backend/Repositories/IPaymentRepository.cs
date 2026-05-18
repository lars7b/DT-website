using Backend.Models;

namespace Backend.Repositories;

public interface IPaymentRepository
{
    public Task<List<Payment>> GetAll(long userid,CancellationToken cancellationToken);
    public Task<Payment?> GetById(long id,long? userid,CancellationToken cancellationToken);
    public Task<bool> Add(Payment payment);
    public Task<bool> Update(Payment payment);
    public Task<bool> Delete(long id);

    public Task<List<Payment>> GetByOrderId(long orderId,CancellationToken token = default);
    public Task<List<Payment>> GetByUser(long userId,CancellationToken token = default);

    public Task<decimal> GetAmountForOrder(long orderId,CancellationToken token = default);
}