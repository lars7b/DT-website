using Backend.Models;
namespace Backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(long id, long? userId);
    Task<List<Order>> GetOrdersAsync(long? userId);
    Task<bool> CreateOrder(long userid);
    Task<bool> UpdateOrder(Order order);
    Task<bool> DeleteOrder(long id);
}