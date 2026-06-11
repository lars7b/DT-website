using Backend.Models;
using Backend.DTOs;

namespace Backend.Services;

public interface IEmployeeService
{
    Task<EmployeeDto?> GetEmployeeProfileAsync(int userId);
    Task<(bool Success, string Message)> UpdateEmployeeAsync(int employeeId, EmployeeDto request);
    Task<CustomerAdminDto?> GetCustomerByEmailAsync(string email);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
}