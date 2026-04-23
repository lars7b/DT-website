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

    public async Task<bool> CreateOrderAsync(long userid)
    {

        // Order order = new Order
        // {
        //     CustomerId = userid,
        //     OrderDate = DateTime.UtcNow,
        //     Status = "Pending"
        //     // add each item from cart
        // };
        return await _orderRepository.CreateOrder(userid);
    }
    public async Task<bool> UpdateOrderAsync(Order order, long userid)
    {
        // check if not paid yet 
        // check if user and if admin
        throw new NotImplementedException();
    }
    public async Task<bool> DeleteOrderAsync(long id, long userId){
        Order? order = await _orderRepository.GetOrderByIdAsync(id);
        // check for admin
        if(order == null || order.CustomerId != userId && order.Status == "Pending"){
            return false;
        }
        return await _orderRepository.DeleteOrder(id);
    }
}
