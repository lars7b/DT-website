using Backend.Models;
namespace Backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(long id);
    Task<List<Order>> GetOrdersAsync();
    Task<bool> CreateOrder(long userid);
    Task<bool> UpdateOrder(Order order);
    Task<bool> DeleteOrder(long id);
}