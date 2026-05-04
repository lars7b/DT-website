using Backend.Models;
using Dapper;
using Npgsql;

namespace Backend.Repositories;

/// <summary>
/// Repository for managing payments in the database.
/// the attributes are amount, payment_date, payment_method, order_id ,status
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly NpgsqlConnection _connection;

    public PaymentRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task<List<Payment>> GetAll(long userid, CancellationToken cancellationToken)
    {
        var payments = await _connection.QueryAsync<Payment>(
            """
            SELECT 
                p.id,
                p.amount,
                p.payment_date,
                p.payment_method,
                p.order_id,
                p.status
            FROM payments AS p
            JOIN orders AS o ON p.order_id = o.id 
            JOIN customers AS c ON c.id=o.customer_id 
            JOIN users AS u ON u.id=c.user_id
            WHERE (c.id = @userId AND u.role != 'Admin') OR u.role = 'Admin';
            """,
            new { userId = userid }
        );
        return payments.ToList();
    }

    public async Task<Payment?> GetById(long id, long? userid, CancellationToken cancellationToken)
    {
        // if no user id is given then the user is admin
        if (userid == null)
        {
            Payment? paymentNoUserid = await _connection.QueryFirstOrDefaultAsync<Payment>(
                "SELECT * FROM payments WHERE id = @id;",
                new { id }
            );
            return paymentNoUserid;
        }
        Payment? payment = await _connection.QueryFirstOrDefaultAsync<Payment>(
            """
            SELECT p.*
            FROM payments p
            JOIN orders o ON p.order_id = o.id
            JOIN customers c ON o.customer_id = c.id
            WHERE p.id = @id AND c.user_id = @userId
            """,
            new { id, userId = userid }
        );
        return payment;
    }

    public async Task<bool> Add(Payment payment)
    {
        string query =
            "INSERT INTO payments (amount, payment_date, payment_method, order_id, status) VALUES (@Amount, @PaymentDate, @PaymentMethod, @OrderId, @Status);";
        var result = await _connection.ExecuteAsync(query, payment);
        return result > 0;
    }

    public async Task<bool> Update(Payment payment)
    {
        string query =
            "UPDATE payments SET amount = @Amount, payment_date = @PaymentDate, payment_method = @PaymentMethod, order_id = @OrderId, status = @Status WHERE id = @Id;";
        var result = await _connection.ExecuteAsync(query, payment);
        return result > 0;
    }

    public async Task<bool> Delete(long Id)
    {
        string query = "DELETE FROM payments WHERE id = @Id;";
        var result = await _connection.ExecuteAsync(query, new { Id });
        return result > 0;
    }

    public async Task<List<Payment>> GetByOrderId(long orderId)
    {
        string query = "SELECT * FROM payments WHERE order_id = @orderId;";
        var payments = await _connection.QueryAsync<Payment>(query, new { orderId });
        return payments.ToList();
    }

    public async Task<List<Payment>> GetByUser(long userId)
    {
        string query = """
            SELECT 
                p.id,
                p.amount,
                p.payment_date,
                p.payment_method,
                p.order_id,
                p.status
            FROM payments AS p 
            JOIN orders AS o ON p.order_id = o.id 
            JOIN customers AS c ON c.id=o.customer_id 
            WHERE c.id=@userId;
            """;
        var payments = await _connection.QueryAsync<Payment>(query, new { userId });
        return payments.ToList();
    }

    public async Task<decimal> GetAmountForOrder(long orderId)
    {
        string query = """
            SELECT SUM(order_items.price) 
            FROM orders AS o 
            JOIN order_items ON o.id = order_items.order_id 
            WHERE o.id=@orderId 
            GROUP BY o.id;
            """;
        decimal amount = await _connection.ExecuteScalarAsync<decimal>(query, new { orderId });
        return amount;
    }
}
