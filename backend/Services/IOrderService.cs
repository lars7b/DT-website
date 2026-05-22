using Backend.DTOs;

namespace Backend.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(long id, long userId, CancellationToken token);
    Task<List<OrderDto>> GetOrdersAsync(long userId, CancellationToken token);
    Task<bool> CreateOrderAsync(long userid);
    Task<bool> UpdateOrderAsync(OrderDto order, long userId);
    Task<bool> DeleteOrderAsync(long id, long userId);
    Task<bool> CancelOrderAsync(long id, long userId);
}
