using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Order?> GetOrderByIdAsync(long id, long userid)
    {
        return await _orderRepository.GetOrderByIdAsync(id);
    }

    public async Task<List<Order>> GetOrdersAsync(long userid)
    {
        return await _orderRepository.GetOrdersAsync();
    }
}
