using Backend.Models;
using Backend.DTOs;

namespace Backend.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerAsync(int userId);
    Task<bool> UpdateCustomerAsync(int userId, CustomerDto customer);
    Task<bool> DeleteCustomerAsync(int userId);
}