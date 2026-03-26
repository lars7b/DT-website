using Backend.Models;

namespace Backend.Services;

public interface IPaymentService
{
    Task<Payment>? GetPaymentByIdAsync(long id);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    Task<bool> CreatePaymentAsync(Payment payment);
    Task<bool> CreatePaymentAsync(long orderId, string method);
    Task<bool> UpdatePaymentAsync(Payment payment);
    Task<bool> DeletePaymentAsync(long id);
}
