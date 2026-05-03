using Backend.Models;

namespace Backend.Services;

public interface IPaymentService
{
    Task<Payment?> GetPaymentByIdAsync(long id,long userid, CancellationToken token);
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(long userid,CancellationToken token);
    Task<bool> CreatePaymentAsync(long userid,Payment payment);
    /// <summary>
    /// updates payment, only admin can update payment
    /// </summary>
    /// <param name="payment">payment that needs to be updated woth the information that will get updated</param>
    /// <param name="token"></param>
    /// <returns>if updating was succesful</returns>
    Task<bool> UpdatePaymentAsync(Payment payment, CancellationToken token);
    /// <summary>
    /// deletes payment, only admin can delete payment
    /// </summary>
    /// <param name="id">id of payment that will be deleted</param>
    /// <param name="token"></param>
    /// <returns>if deleting was succesful</returns>
    Task<bool> DeletePaymentAsync(long id, CancellationToken token);
}
