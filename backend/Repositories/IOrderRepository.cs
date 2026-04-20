using Backend.Models;
namespace Backend.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetOrderByIdAsync(long id);
    Task<List<Order>> GetOrdersAsync();
}