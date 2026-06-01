using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public sealed class OrderHistoryRepository
{
    private readonly string _connectionString;

    public OrderHistoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT 1
                             FROM customers
                             WHERE id = @customerId";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", customerId);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<IReadOnlyList<OrderHistory>> GetOrderHistoryAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT o.id AS order_id,
                                    o.order_date,
                                    o.status,
                                    oi.id AS order_item_id,
                                    oi.product_id,
                                    oi.quantity,
                                    oi.price,
                                    p.name AS product_name
                             FROM orders o
                             LEFT JOIN order_items oi ON oi.order_id = o.id
                             LEFT JOIN products p ON p.id = oi.product_id
                             WHERE o.customer_id = @customerId
                             ORDER BY o.order_date DESC, o.id DESC, oi.id ASC";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var ordersById = new Dictionary<int, OrderHistory>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var orderId = reader.GetInt32(reader.GetOrdinal("order_id"));
            if (!ordersById.TryGetValue(orderId, out var order))
            {
                order = new OrderHistory
                {
                    OrderId = orderId,
                    OrderDate = reader.GetDateTime(reader.GetOrdinal("order_date")),
                    Status = reader.IsDBNull(reader.GetOrdinal("status"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("status"))
                };

                ordersById.Add(orderId, order);
            }

            if (!reader.IsDBNull(reader.GetOrdinal("order_item_id")))
            {
                order.Items.Add(new OrderHistoryItem
                {
                    OrderItemId = reader.GetInt32(reader.GetOrdinal("order_item_id")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                    ProductName = reader.IsDBNull(reader.GetOrdinal("product_name"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("product_name")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("price"))
                });
            }
        }

        return ordersById.Values.ToList();
    }
}
