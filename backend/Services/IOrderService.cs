using Backend.Models;
namespace Backend.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(long id, long userId);
    Task<List<Order>> GetOrdersAsync(long userId);
    Task<bool> CreateOrderAsync(long userid);
    Task<bool> UpdateOrderAsync(Order order, long userId);
    Task<bool> DeleteOrderAsync(long id, long userId);
}