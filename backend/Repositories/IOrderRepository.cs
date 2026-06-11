using Backend.Models;
namespace Backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(long id, long? userId,CancellationToken token = default);
    Task<List<Order>> GetOrdersAsync(long? userId,CancellationToken token = default);
    Task<bool> CreateOrder(long userid);
    Task<bool> UpdateOrder(Order order);
    Task<bool> DeleteOrder(long id);
    Task<List<OrderStatusHistory>> GetHistoryByOrderIdAsync(long orderid);
}