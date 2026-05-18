using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class CustomerService : ICustomerService
{
    private readonly CustomerRepository _repository;

    public CustomerService(CustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _repository.GetAllCustomersAsync();
    }
}