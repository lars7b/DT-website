using Backend.DTOs;

namespace Backend.Services;

public interface IPaymentService
{
    Task<PaymentDto?> GetPaymentByIdAsync(long id,long userid, CancellationToken token);
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(long userid,CancellationToken token);
    Task<bool> CreatePaymentAsync(long userid,CreatePaymentDto payment);
    /// <summary>
    /// updates payment, only admin can update payment
    /// </summary>
    /// <param name="payment">payment that needs to be updated woth the information that will get updated</param>
    /// <param name="token">optional token to give for selecting am payment that needs to be updated</param>
    /// <returns>if updating was succesful</returns>
    Task<bool> UpdatePaymentAsync(PaymentDto payment, CancellationToken token = default);
    /// <summary>
    /// deletes payment, only admin can delete payment
    /// </summary>
    /// <param name="id">id of payment that will be deleted</param>
    /// <returns>if deleting was succesful</returns>
    Task<bool> DeletePaymentAsync(long id);
}
