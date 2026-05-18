using Backend.Models;
using Backend.DTOs;
using Npgsql;

namespace Backend.Repositories;  
   
public class EmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;

    public EmployeeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public async Task<Customer> GetEmployeeProfileAsync(int employeeId)
    {
        return default;
    }

    public async Task<bool> UpdateEmployeeAsync(int employeeId, EmployeeDto request)
    {
        return default;
    }
}
