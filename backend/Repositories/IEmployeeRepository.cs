using Backend.Models;

namespace Backend.Repositories;

public interface IEmployeeRepository
{
    Task<List<Customer>> GetAllCustomersAsync();
    Task<List<Order>> GetCustomerOrdersAsync(int id);
    Task<bool> DeleteCustomerAsync(int id);
}
