using Backend.Models;

namespace Backend.Services;

public interface IEmployeeService
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer> GetCustomerAsync(int id);
    Task<List<Order>> GetCustomerOrdersAsync(int id);
    Task<bool> DeleteCustomerAsync(int id);
}