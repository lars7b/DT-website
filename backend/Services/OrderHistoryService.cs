using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public sealed class OrderHistoryService : IOrderHistoryService
{
    private readonly OrderHistoryRepository _repository;

    public OrderHistoryService(OrderHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _repository.CustomerExistsAsync(customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderHistory>> GetOrderHistoryAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderHistoryAsync(customerId, cancellationToken);
    }
}
