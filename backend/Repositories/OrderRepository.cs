using Backend.Models;
using Npgsql;
using Dapper;
namespace Backend.Repositories;

/// <summary>
/// Deze repository gaat queries uitvoeren met de orders en order_items tabellen in postgreSQL
/// </summary>
public class OrderRepository : IOrderRepository
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
        throw new NotImplementedException();
        return new Order { };
    }

    public async Task<List<Order>> GetOrdersAsync()
    {
        throw new NotImplementedException();
        return new List<Order> { };
    }

    public async Task<bool> CreateOrder(long userid)
    {
        string query = """
            INSERT INTO orders (customer_id, order_date, status)
            SELECT sc.customer_id, NOW(), 'Pending'
            FROM shopping_carts sc
            WHERE sc.customer_id = @CustomerId;
            """;
        int result = await _connection.ExecuteAsync(query, new { CustomerId = userid });

        return result > 0;
    }

    public async Task<bool> UpdateOrder(Order order)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteOrder(long id)
    {
        throw new NotImplementedException();
    }
}
