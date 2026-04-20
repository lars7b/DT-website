using Backend.Models;
using Npgsql;
namespace Backend.Repositories;

/// <summary>
/// Deze repository gaat queries uitvoeren met de orders en order_items tabellen in postgreSQL
/// </summary>
public class OrderRepository:IOrderRepository
{
    private readonly NpgsqlConnection _connection;

    public OrderRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        _connection = new NpgsqlConnection(connectionString);
    }
    public async Task<Order?> GetOrderByIdAsync(long id)
    {
        return new Order{};
    }
    
    public async Task<List<Order>> GetOrdersAsync()
    {
        return new List<Order>{};
    }

    public async Task<bool> CreateOrder(Order order){throw new NotImplementedException();}
    public async Task<bool> UpdateOrder(Order order){throw new NotImplementedException();}
    public async Task<bool> DeleteOrder(Order order){throw new NotImplementedException();}

}