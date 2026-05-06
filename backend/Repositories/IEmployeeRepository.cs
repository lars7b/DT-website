using Backend.Models;
using Backend.DTOs;

namespace Backend.Repositories;

public interface IEmployeeRepository
{
    Task<Customer> GetEmployeeProfileAsync(int employeeId);
    Task<bool> UpdateEmployeeAsync(int employeeId, EmployeeDto request);
}
