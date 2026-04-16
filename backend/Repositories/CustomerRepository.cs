using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public async Task<Customer> GetCustomerAsync(int id)
    {
        return default;
    }

    public async Task<bool> UpdateCustomerAsync(int id, Customer customer)
    {
        return default;
    }

    public async Task<List<Order>> GetCustomerOrdersAsync(int id)
    {
        return default;
    }
}
