using Backend.Models;

namespace Backend.Services;

public interface ICustomerService
{
    Task<Customer> GetCustomerAsync(int id);
    Task<bool> UpdateCustomerAsync(int id, Customer customer);
    Task<List<Order>> GetCustomerOrdersAsync(int id);
}