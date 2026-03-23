using Backend.Models;

namespace Backend.Services;

public interface ICustomerService
{
    Task<List<Customer>> GetAllCustomersAsync();
}