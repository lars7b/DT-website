using Backend.Models;

namespace Backend.Services;

public interface IOrderHistoryService
{
    Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderHistory>> GetOrderHistoryAsync(int customerId, CancellationToken cancellationToken = default);
}
