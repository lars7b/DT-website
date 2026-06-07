using Backend.Models;
using Backend.DTOs;

namespace Backend.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeDto?> GetEmployeeProfileAsync(int employeeId);
    Task<bool> UpdateEmployeeAsync(int employeeId, EmployeeDto request);
    Task<CustomerAdminDto?> GetCustomerByEmailAsync(string email);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
}
