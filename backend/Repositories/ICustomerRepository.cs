using Backend.Models;

namespace Backend.Repositories;

public interface ICustomerRepository
{
    Task<Customer> GetCustomerAsync(int id);
    Task<bool> UpdateCustomerAsync(int id, Customer customer);
    Task<List<Order>> GetCustomerOrdersAsync(int id);
}