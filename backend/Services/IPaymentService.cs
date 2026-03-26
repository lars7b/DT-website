namespace Backend.Services;

public interface IPaymentService
{
    Task<Payment> GetPaymentByIdAsync(long id);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync();
    Task<Payment> CreatePaymentAsync(Payment payment);
    Task<Payment> CreatePaymentAsync(long orderId, string method);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(long id);
}