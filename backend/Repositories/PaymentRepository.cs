using Backend.Models;
using Npgsql;
using Dapper;
namespace Backend.Repositories;

public class PaymentRepository
{
    protected static readonly string _table = "payments";
    protected static readonly string _attributes = "amount, payment_date, payment_method, order_id";
    private readonly NpgsqlConnection db;

    public PaymentRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        db = new NpgsqlConnection(connectionString);
    }

    public async Task<List<Payment>> GetAll()
    {
        var payments = await db.QueryAsync<Payment>($"SELECT * FROM {_table}");
        return payments.ToList();
    }
    public async Task<Payment?> GetById(long id)
    {
        var payment = await db.QueryFirstOrDefaultAsync<Payment>(
            $"SELECT * FROM {_table} WHERE id = @id",
            new { id }
        );
        return payment;
    }
    public async Task<bool> Add(Payment payment)
    {
        string query = $"INSERT INTO {_table} ({_attributes}) VALUES (@Amount, @PaymentDate, @PaymentMethod, @OrderId);";
        var result = await db.ExecuteAsync(query, payment);
        return result > 0;
    }
    public async Task<bool> Update(Payment payment)
    {
        string query = $"UPDATE {_table} SET amount = @Amount, payment_date = @PaymentDate, payment_method = @PaymentMethod, order_id = @OrderId WHERE id = @Id;";
        var result = await db.ExecuteAsync(query, payment);
        return result > 0;
    }
    public async Task<bool> Delete(Payment? payment)
    {
        if (payment == null) return false;
        string query = $"DELETE FROM {_table} WHERE id = @Id;";
        var result = await db.ExecuteAsync(query, new { payment.Id });
        return result > 0;
    }

    public List<Payment> GetByOrderId(long orderId)
    {
        string query = $"SELECT * FROM {_table} WHERE order_id = @orderId;";
        return new List<Payment>();
    }

    public List<Payment> GetByUser(long userId)
    {
        throw new NotImplementedException();
        //  payment -> order -> customers
        string query = "";
        return new List<Payment>();
    }
}
