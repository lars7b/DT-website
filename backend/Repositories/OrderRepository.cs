using Backend.Models;
using Dapper;
using Npgsql;

namespace Backend.Repositories;

/// <summary>
/// Deze repository gaat queries uitvoeren met de orders en order_items tabellen in postgreSQL
/// orders bevat "id", "customer_id","order_date","status"
/// order_items bevat id, order_id, "product_id", "quantity", "price"
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public async Task<Order?> GetOrderByIdAsync(
        long id,
        long? userId,
        CancellationToken token = default
    )
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

        string sql = """
            SELECT
                o.id,
                o.customer_id AS CustomerId,
                o.order_date AS OrderDate,
                o.status,

                items.id AS ItemSplitId,

                items.id,
                items.order_id AS OrderId,
                items.product_id AS ProductId,
                items.quantity,
                items.price,

                p.id AS ProductId,

                p.id,
                p.name,
                p.description,
                p.price

            FROM orders o
            JOIN order_items items
                ON items.order_id = o.id 
            JOIN products p
                ON p.id = items.product_id 
            """;

        if (userId != null)
        {
            sql += """
                 INNER JOIN customers c
                    ON c.id = o.customer_id
                WHERE
                    o.id = @id
                    AND (
                        c.user_id = @userId
                        OR EXISTS (
                            SELECT 1
                            FROM users admin_user
                            WHERE admin_user.id = @userId
                            AND admin_user.role = 'Admin'
                        )
                    )
                """;
        }
        else
        {
            sql += """
                WHERE o.id = @id
                """;
        }

        Order? result = null;

        await connection.QueryAsync<Order, OrderItem,Product, Order>(
            sql,
            (order, item, product) =>
            {
                if (result == null)
                {
                    result = order;
                    result.Items = new List<OrderItem>();
                }
                item.Product = product;
                result.Items.Add(item);

                return result;
            },
            new { id, userId },
            splitOn: "ItemSplitId,ProductId"
        );

        return result;
    }

    public async Task<OrderItem?> GetOrderItemByIdAsync(
        long id,
        long? userId,
        CancellationToken token = default
    )
    { //
        await using NpgsqlConnection _connection = new NpgsqlConnection(_connectionString);
        OrderItem? order = await _connection.QueryFirstOrDefaultAsync<OrderItem>(
            """
            SELECT items.id, items.order_id AS orderid, items.product_id AS productid, items.quantity, items.price
            FROM order_items AS items
            JOIN orders AS o ON items.order_id = o.id 
            JOIN customers ON customers.id = o.customer_id 
            WHERE o.id = @id AND (
                customers.user_id = @userId
                OR EXISTS (
                    SELECT 1
                    FROM users admin_user
                    WHERE admin_user.id = @userId
                    AND admin_user.role = 'Admin');
            """,
            new { userId, id }
        );
        return order;
    }

    public async Task<List<Order>> GetOrdersAsync(long? userId, CancellationToken token = default)
    {
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);

        string sql = """
            SELECT
                o.id,
                o.customer_id AS CustomerId,
                o.order_date AS OrderDate,
                o.status,

                items.id AS ItemSplitId,

                items.id,
                items.order_id AS OrderId,
                items.product_id AS ProductId,
                items.quantity,
                items.price,

                p.id AS ProductId,

                p.id,
                p.name,
                p.description,
                p.price

            FROM orders o
            JOIN order_items items
                ON o.id = items.order_id  
            JOIN products p
                ON p.id = items.product_id  
            """;

        if (userId != null)
        {
            sql += """
                   INNER JOIN customers c
                    ON c.id = o.customer_id
                INNER JOIN users u
                    ON u.id = c.user_id
                WHERE
                    c.user_id = @userId
                    OR EXISTS (
                        SELECT 1
                        FROM users admin_user
                        WHERE admin_user.id = @userId
                        AND admin_user.role = 'Admin'
                    )
                Order BY o.order_date DESC
                """;
        }

        var orderDict = new Dictionary<long, Order>();

        await connection.QueryAsync<Order, OrderItem, Product, Order>(
            sql,
            (order, item, product) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.Items = new List<OrderItem>();

                    orderDict.Add(existingOrder.Id, existingOrder);
                }

                item.Product = product;
                existingOrder.Items.Add(item);

                return existingOrder;
            },
            new { userId },
            splitOn: "ItemSplitId,ProductId"
        );

        return orderDict.Values.ToList();
    }

    public async Task<bool> CreateOrder(long userId)
    {
        await using NpgsqlConnection _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
        using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            long orderId = await _connection.ExecuteScalarAsync<long>(
                """
                INSERT INTO orders (customer_id, order_date, status)
                SELECT c.id, NOW(), 'Pending'
                FROM customers c
                JOIN users u ON u.id = c.user_id
                WHERE u.id = @userId
                RETURNING id;
                """,
                new { userId },
                transaction
            );
            if (orderId == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }
            int resultitems = await _connection.ExecuteAsync(
                """
                INSERT INTO order_items (order_id, product_id, quantity, price)
                SELECT 
                    @orderId,
                    ci.product_id,
                    ci.quantity,
                    p.price
                FROM cart_items ci
                JOIN shopping_carts AS sc ON sc.id = ci.cart_id
                JOIN products AS p ON p.id = ci.product_id
                JOIN customers AS c ON sc.customer_id = c.id
                JOIN users AS u ON u.id = c.user_id
                WHERE u.id = @userId;
                """,
                new { orderId, userId },
                transaction
            );
            if (resultitems == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }
            await _connection.ExecuteAsync(
                """
                DELETE FROM cart_items
                WHERE cart_id IN (
                    SELECT sc.id FROM shopping_carts AS sc
                    JOIN customers AS c ON sc.customer_id = c.id
                    JOIN users AS u ON u.id = c.user_id   
                    WHERE u.id = @userId
                );
                """,
                new { userId },
                transaction
            );
            resultitems += await _connection.ExecuteAsync(
                """
                INSERT INTO order_status_history (order_id, status, status_date)
                VALUES (@orderId, @status, CURRENT_DATE);
                """,
                new { orderId, status = "Pending" },
                transaction
            );
            if (orderId != 0)
            {
                resultitems++;
            }
            await transaction.CommitAsync();
            return resultitems > 2;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// only status gets updated
    /// orderdate and customer id arent being updated
    /// the reason is that customer should stay the same and orderdate is date of creation
    /// </summary>
    /// <param name="order">the order you want to update</param>
    /// <returns>if the updating was succesful</returns>
    public async Task<bool> UpdateOrder(Order order)
    {
        await using NpgsqlConnection _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
        using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            string query = """
                UPDATE orders
                SET status = @Status
                WHERE orders.id = @Id;
                """;
            int result = await _connection.ExecuteAsync(
                query,
                new { Status = order.Status, Id = order.Id },
                transaction
            );
            if (result == 0)
            {
                return false;
            }
            result += await _connection.ExecuteAsync(
                """
                INSERT INTO order_status_history (order_id, status, status_date)
                VALUES (@orderId, @status, current_timestamp);
                """,
                new { orderId = order.Id, status = order.Status },
                transaction
            );
            await transaction.CommitAsync();
            return result == 2;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteOrder(long id)
    {
        // order items also get deleted bc its on delete cascade with order_id
        await using NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        string query = "DELETE FROM orders WHERE id = @Id;";
        int result = await connection.ExecuteAsync(query, new { id });
        return result == 1;
    }
}
