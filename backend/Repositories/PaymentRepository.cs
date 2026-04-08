using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class PaymentRepository : RepositoryBase<Payment>
{
    protected static readonly string _table = "payments";
    protected static readonly string _attributes = "amount, payment_date, payment_method, order_id";

    public PaymentRepository(IConfiguration configuration)
        : base(configuration, _table, _map, _attributes,_reverseMap) { }

    public List<Payment> GetByOrderId(long orderId)
    {
        string query = $"SELECT * FROM {_table} WHERE order_id = @orderId;";
        return new List<Payment>();
    }

    public List<Payment> GetByUser(long userId)
    {
        //  payment -> order -> customers
        string query = "";
        return new List<Payment>();
    }

    private static Payment _map(NpgsqlDataReader reader)
    {
        return new Payment
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
            PaymentDate = reader.GetDateTime(reader.GetOrdinal("payment_date")),
            PaymentMethod = reader.GetString(reader.GetOrdinal("payment_method")),
            OrderId = reader.GetInt64(reader.GetOrdinal("order_id")),
        };
    }
    private static readonly Dictionary<string, string> _reverseMap = new()
    {
        { "Amount", "amount" },
        { "PaymentDate", "payment_date" },
        { "PaymentMethod", "payment_method" },
        { "OrderId", "order_id" }
    };
}
