using Backend.Models;
using Backend.DTOs;

namespace Backend.Services;

public interface IEmployeeService
{
    Task<EmployeeDto> GetEmployeeProfileAsync(int employeeId);
    Task<(bool Success, string Message)> UpdateEmployeeAsync(int employeeId, EmployeeDto request);
}