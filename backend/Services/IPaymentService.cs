using Backend.Models;

namespace Backend.Services;

public interface IPaymentService
{
    Task<Payment?> GetPaymentByIdAsync(long id, CancellationToken token);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(CancellationToken token);
    Task<bool> CreatePaymentAsync(Payment payment);
    Task<bool> CreatePaymentAsync(long orderId, string method);
    Task<bool> UpdatePaymentAsync(Payment payment, CancellationToken token);
    Task<bool> DeletePaymentAsync(long id, CancellationToken token);
}
