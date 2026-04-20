using Backend.Models;
namespace Backend.Services;

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(long id, long userId);
    Task<List<Order>> GetOrdersAsync(long userId);
}