using Backend.DTOs;
using Backend.Repositories;

namespace Backend.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto?> GetEmployeeProfileAsync(int userId)
    {
        return await _employeeRepository.GetEmployeeProfileAsync(userId);
    }

    public async Task<(bool Success, string Message)> UpdateEmployeeAsync(int employeeId, EmployeeDto request)
    {
        return default;
    }

    public async Task<CustomerAdminDto?> GetCustomerByEmailAsync(string email)
    {
        return await _employeeRepository.GetCustomerByEmailAsync(email);
    }
    
    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        return await _employeeRepository.GetOrdersByUserIdAsync(userId);
    }
}