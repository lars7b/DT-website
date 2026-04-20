using Backend.Models;
using Npgsql;
using Dapper;
namespace Backend.Repositories;

public class PaymentRepository
{
    protected static readonly string _table = "payments";
    protected static readonly string _attributes = "amount, payment_date, payment_method, order_id";
    private readonly NpgsqlConnection _connection;

    public PaymentRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task<List<Payment>> GetAll()
    {
        var payments = await _connection.QueryAsync<Payment>($"SELECT * FROM {_table}");
        return payments.ToList();
    }
    public async Task<Payment?> GetById(long id)
    {
        var payment = await _connection.QueryFirstOrDefaultAsync<Payment>(
            $"SELECT * FROM {_table} WHERE id = @id",
            new { id }
        );
        return payment;
    }
    public async Task<bool> Add(Payment payment)
    {
        string query = $"INSERT INTO payments ({_attributes}) VALUES (@Amount, @PaymentDate, @PaymentMethod, @OrderId);";
        var result = await _connection.ExecuteAsync(query, payment);
        return result > 0;
    }
    public async Task<bool> Update(Payment payment)
    {
        string query = $"UPDATE payments SET amount = @Amount, payment_date = @PaymentDate, payment_method = @PaymentMethod, order_id = @OrderId WHERE id = @Id;";
        var result = await _connection.ExecuteAsync(query, payment);
        return result > 0;
    }
    public async Task<bool> Delete(Payment? payment)
    {
        if (payment == null) return false;
        string query = $"DELETE FROM payments WHERE id = @Id;";
        var result = await _connection.ExecuteAsync(query, new { payment.Id });
        return result > 0;
    }

    public async Task<List<Payment>> GetByOrderId(long orderId)
    {
        string query = $"SELECT * FROM payments WHERE order_id = @orderId;";
        var payments = await _connection.QueryAsync<Payment>(query,new { orderId });
        return payments.ToList();
    }

    public async Task<List<Payment>> GetByUser(long userId)
    {
        string query = $"SELECT * FROM payments AS p JOIN orders AS o ON p.order_id = o.id JOIN users AS u ON u.id=o.customer_id WHERE u.id=@userId;";
        var payments = await _connection.QueryAsync<Payment>(query, new{userId});
        return payments.ToList();
    }

    public async Task<decimal> GetAmountForOrder(Order order)
    {
        string query = "SELECT SUM(order_items.price) FROM order AS o JOIN order_items ON o.id = order_items.order_id WHERE o.id=@Id GROUP BY o.id;";
        decimal amount = await _connection.ExecuteScalarAsync<decimal>(query, new{order.Id});
        return amount;
    }

    //TODO
    // Env.Load();
    // var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
    // NpgsqlConnection _connection = new NpgsqlConnection(connectionString);

    // // Example: Query rows
    // var sql = "SELECT id, first_name as FirstName, email FROM students";
    // var students = _connection.Query<Student>(sql);

    // foreach (var s in students)
    // {
    //     Console.WriteLine($"{s.Id} - {s.FirstName} - {s.Email}");
    // }
}
