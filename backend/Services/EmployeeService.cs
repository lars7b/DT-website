using Backend.DTOs;
using Backend.Repositories;
using Isopoh.Cryptography.Argon2;

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

    public async Task<(bool Success, string Message)> CreateEmployeeAsync(CreateEmployeeDto request)
    {
        string passwordHash = Argon2.Hash(request.Password);

        var result = await _employeeRepository.CreateEmployeeAsync(request, passwordHash);

        if (!result)
        {
            return (false, "E-mailadres of telefoonnummer is al in gebruik.");
        }

        return (true, $"Medewerker succesvol aangemaakt.");
    }
}