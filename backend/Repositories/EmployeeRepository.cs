using Backend.Models;
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

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        /*
        var customers = new List<Customer>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, first_name, last_name, email, phone, address FROM customers", connection);
        
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            customers.Add(new Customer
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Email = reader.GetString(3), //null?
                Phone = reader.GetString(4), //null?
                Address = reader.GetString(5) //null?
            });
        }

        return customers;
        */
        return default;
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int id)
    {
        return default;
    }

        public async Task<bool> DeleteCustomerAsync(int id)
    {
        return default;
    }
}
