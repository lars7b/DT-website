using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<EmployeeDto> GetEmployeeProfileAsync(int employeeId)
    {
        return default;
    }

    public async Task<(bool Success, string Message)> UpdateEmployeeAsync(int employeeId, EmployeeDto request)
    {
        return default;
    }
}