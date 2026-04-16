using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class CustomerService : ICustomerService
{
private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Customer> GetCustomerAsync(int id)
    {
        return await _customerRepository.GetCustomerAsync(id);
    }

    public async Task<bool> UpdateCustomerAsync(int id, Customer customer)
    {
        return await _customerRepository.UpdateCustomerAsync(id, customer);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int id)
    {
    return await _customerRepository.GetCustomerOrdersAsync(id);
    }
}