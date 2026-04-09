using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICustomerRepository _customerRepository;

    public EmployeeService(
        IEmployeeRepository employeeRepository, 
        ICustomerRepository customerRepository)
    {
        _employeeRepository = employeeRepository;
        _customerRepository = customerRepository;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _employeeRepository.GetAllCustomersAsync();
    }

    public async Task<Customer> GetCustomerAsync(int id)
    {
        return await _customerRepository.GetCustomerAsync(id);
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int id)
    {
        return await _employeeRepository.GetCustomerOrdersAsync(id);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        return await _employeeRepository.DeleteCustomerAsync(id);
    }
}