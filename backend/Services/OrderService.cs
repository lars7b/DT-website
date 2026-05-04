using Backend.DTOs;
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

    public async Task<OrderDto?> GetOrderByIdAsync(
        long id,
        long userid,
        CancellationToken token = default
    )
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(id,userid);
        if (order == null)
        {
            return null;
        }
        OrderDto dto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
        };
        return dto;
    }

    public async Task<List<OrderDto>> GetOrdersAsync(long userid, CancellationToken token = default)
    {
        //TODO
        List<Order> orders = await _orderRepository.GetOrdersAsync(userid);
        List<OrderDto> dtos = new List<OrderDto>();
        foreach (Order order in orders)
        {
            OrderDto dto = new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                Status = order.Status,
            };
            dtos.Add(dto);
        }
        return dtos;
    }

    public async Task<bool> CreateOrderAsync(long userid)
    {
        return await _orderRepository.CreateOrder(userid);
    }

    public async Task<bool> UpdateOrderAsync(OrderDto orderdto, long customerid)
    {
        // check if not paid yet
        // check if user and if admin
        Order order = new Order
        {
            CustomerId = customerid,
            OrderDate = DateTime.UtcNow,
            Status = "Pending",
            // add each item from cart or make function for mapping dto and order
        };
        return await _orderRepository.UpdateOrder(order);
    }

    public async Task<bool> DeleteOrderAsync(long id, long userId)
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(id,userId);
        // check for admin
        if (order == null || order.CustomerId != userId && order.Status == "Pending")
        {
            return false;
        }
        return await _orderRepository.DeleteOrder(id);
    }

    public async Task<bool> CancelOrderAsync(long id, long userId)
    {
        Order? order = await _orderRepository.GetOrderByIdAsync(id,userId);
        // check for admin
        if (order == null ||order.Status != "Pending")//|| order.CustomerId != userId )
        {
            return false;
        }
        // if cancelled succesfully should also change storage product in the store back to the amount
        order.Status = "Cancelled";
        return await _orderRepository.UpdateOrder(order);
    }
}
