using Backend.Models;
using Backend.DTOs;

namespace Backend.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetCustomerAsync(int userId);
    Task<(bool Success, string Message)> UpdateCustomerAsync(int id, CustomerDto customer);
    Task<(bool Success, string Message)> DeleteCustomerAsync(int userId, string password);
}