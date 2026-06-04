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
    private readonly string _connectionString;

    public PaymentRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public async Task<List<Payment>> GetAll(long userid, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        IEnumerable<Payment> payments = await connection.QueryAsync<Payment>(
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
            WHERE
                c.user_id = @userId
                OR EXISTS (
                    SELECT 1
                    FROM users admin_user
                    WHERE admin_user.id = @userId
                    AND admin_user.role = 'Admin');
            """,
            new { userId = userid }
        );
        return payments.ToList();
    }

    public async Task<Payment?> GetById(long id, long? userid, CancellationToken cancellationToken)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        // if no user id is given then the user is admin
        if (userid == null)
        {
            Payment? paymentNoUserid = await connection.QueryFirstOrDefaultAsync<Payment>(
                "SELECT * FROM payments WHERE id = @id;",
                new { id }
            );
            return paymentNoUserid;
        }
        Payment? payment = await connection.QueryFirstOrDefaultAsync<Payment>(
            """
            SELECT p.*
            FROM payments p
            JOIN orders o ON p.order_id = o.id
            JOIN customers c ON o.customer_id = c.id
            WHERE p.id = @id AND (
                c.user_id = @userId
                OR EXISTS (
                    SELECT 1
                    FROM users admin_user
                    WHERE admin_user.id = @userId
                    AND admin_user.role = 'Admin');
            """,
            new { id, userId = userid }
        );
        return payment;
    }

    
    public async Task<Payment?> Add(Payment payment)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query =
            // "INSERT INTO payments (amount, payment_date, payment_method, order_id, status) VALUES (@Amount, @PaymentDate, @PaymentMethod, @OrderId, @Status);";
             @"INSERT INTO payments (amount, payment_date, payment_method, order_id, status) VALUES (amount, Current_timestamp, @PaymentMethod, @OrderId, @Status)
                SELECT SUM(order_items.price) as amount
                    FROM orders AS o 
                    JOIN order_items ON o.id = order_items.order_id 
                    WHERE o.id=@OrderId 
                    GROUP BY o.id RETURNING *;";
        Payment? result = await connection.QuerySingleAsync<Payment>(query, payment);
        return result;
    }

    public async Task<bool> Update(Payment payment)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query =
            "UPDATE payments SET amount = @Amount, payment_date = @PaymentDate, payment_method = @PaymentMethod, order_id = @OrderId, status = @Status WHERE id = @Id;";
        int result = await connection.ExecuteAsync(query, payment);
        return result > 0;
    }

    public async Task<bool> Delete(long Id, CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = "DELETE FROM payments WHERE id = @Id;";
        int result = await connection.ExecuteAsync(query, new { Id });
        return result > 0;
    }

    public async Task<List<Payment>> GetByOrderId(long orderId,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = "SELECT * FROM payments WHERE order_id = @orderId;";
        IEnumerable<Payment> payments = await connection.QueryAsync<Payment>(query, new { orderId });
        return payments.ToList();
    }

    public async Task<List<Payment>> GetByUser(long userId,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
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
        IEnumerable<Payment> payments = await connection.QueryAsync<Payment>(query, new { userId });
        return payments.ToList();
    }

    public async Task<decimal> GetAmountForOrder(long orderId,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = """
            SELECT SUM(order_items.price) 
            FROM orders AS o 
            JOIN order_items ON o.id = order_items.order_id 
            WHERE o.id=@orderId 
            GROUP BY o.id;
            """;
        decimal amount = await connection.ExecuteScalarAsync<decimal>(query, new { orderId });
        return amount;
    }
    public async Task<long?> GetPendingOrderIdForUser(long userId,CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = """
            SELECT o.id 
            FROM orders AS o 
            JOIN customers AS c ON o.customer_id = c.id 
            WHERE c.user_id = @userId AND o.status = 'Pending' 
            LIMIT 1;
            """;
        long? orderId = await connection.ExecuteScalarAsync<long?>(query, new { userId });
        return orderId;
    }
}
