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

    public async Task<Order?> GetOrderByIdAsync(long id, long? userId)
    { //
        await using var _connection = new NpgsqlConnection(_connectionString);

        if (userId == null)
        {
            string sql = """
                SELECT o.id, o.customer_id AS customerid ,o.order_date AS orderdate,o.status
                FROM orders AS o
                JOIN order_items AS items ON items.order_id = o.id 
                WHERE o.id = @id;
                """;
            Order? order = await _connection.QueryFirstOrDefaultAsync<Order>(sql, new { id });
            return order;
        }
        else
        {
            string sql = """
                SELECT o.id, o.customer_id AS customerid ,o.order_date AS orderdate,o.status
                FROM orders AS o
                JOIN order_items AS items ON items.order_id = o.id 
                JOIN customers ON customers.id = o.customer_id 
                JOIN users AS u ON customers.user_id = u.id
                WHERE (customers.user_id = @userId OR u.role = 'Admin') AND o.id = @id;
                """;
            Order? order = await _connection.QueryFirstOrDefaultAsync<Order>(
                sql,
                new { userId, id }
            );
            return order;
        }
        // throw new NotImplementedException();
        // https://stackoverflow.com/questions/7508322/how-do-i-map-lists-of-nested-objects-with-dapper

        // var sql =
        //     @"SELECT o.*,items.*
        //         FROM orders AS o
        //         INNER JOIN order_items AS items ON o.id = items.order_id
        //         WHERE o.id = @id AND o.customer_id = @userId";// add for admin

        // // var sql =
        // //     @"SELECT o.*,items.*
        // //         FROM orders AS o
        // //         INNER JOIN order_items AS items ON o.Id = items.order_id";
        // var orders = await _connection.QueryAsync<Order, List<OrderItem>, Order?>(
        //     sql,
        //     (order, items) =>
        //     { // userid will make problem fix TODO
        //         order.Items = items;
        //         // if(order.Id=id && order.CustomerId=userId){return order;}
        //         return null;
        //     },
        //     splitOn: "order_id"
        // );
        // return orders.First();
    }

    public async Task<OrderItem?> GetOrderItemByIdAsync(long id, long? userId)
    { //
        await using var _connection = new NpgsqlConnection(_connectionString);
        OrderItem? order = await _connection.QueryFirstOrDefaultAsync<OrderItem>(
            """
            SELECT items.id, items.order_id AS orderid, items.product_id AS productid, items.quantity, items.price
            FROM order_items AS items
            JOIN orders AS o ON items.order_id = o.id 
            JOIN customers ON customers.id = o.customer_id 
            WHERE customers.id = @userId AND o.id = @id;
            """,
            new { userId, id }
        );
        return order;
        throw new NotImplementedException();
    }

    public async Task<List<Order>> GetOrdersAsync(long? userId)
    {
        // https://stackoverflow.com/questions/7472088/correct-use-of-multimapping-in-dapper
        // https://www.learndapper.com/relationships
        // https://dappertutorial.net/query
        await using var _connection = new NpgsqlConnection(_connectionString);
        string sql =
            @"SELECT 
            o.id, 
            o.customer_id AS customerid,
            o.order_date AS orderdate,
            o.status, 
            items.id, 
            items.order_id AS orderid, 
            items.product_id AS productid, 
            items.quantity, 
            items.price
                FROM orders AS o 
                INNER JOIN order_items AS items ON o.Id = items.order_id";

        // IEnumerable<Order> orders = await _connection.QueryAsync<Order, List<OrderItem>, Order>(
        //     sql,
        //     (order, items) =>
        //     {
        //         order.Items = items;
        //         return order;
        //     },
        //     splitOn: "orderid"
        // );
        // return orders.ToList();

        var orderDict = new Dictionary<long, Order>();

        var orders = await _connection.QueryAsync<Order, OrderItem, Order>(
            sql,
            (order, item) =>
            {
                if (!orderDict.TryGetValue(order.Id, out var existingOrder))
                {
                    existingOrder = order;
                    existingOrder.Items = new List<OrderItem>();
                    orderDict.Add(existingOrder.Id, existingOrder);
                }

                existingOrder.Items.Add(item);
                return existingOrder;
            },
            splitOn: "orderid"
        );

        return orderDict.Values.ToList();

        // var items = await _connection.QueryAsync<OrderItem>(
        //     """
        //     SELECT items.* FROM order_items AS items
        //     JOIN orders AS o ON items.order_id = o.id
        //     JOIN customers ON customers.id = o.customer_id
        //     WHERE customers.id = @userId;
        //     """,
        //     new { userId }
        // );
        // return items.ToList();
        ///
        // var items = await _connection.QueryAsync<Order>(
        //     """
        //     SELECT o.*, items.*
        //     FROM order_items AS items
        //     JOIN orders AS o ON items.order_id = o.id
        //     JOIN customers ON customers.id = o.customer_id
        //     WHERE customers.id = @userId;
        //     """,
        //     new { userId }
        // );
        // return items.ToList();
        throw new NotImplementedException();
    }

    public async Task<bool> CreateOrder(long userId)
    {
        await using var _connection = new NpgsqlConnection(_connectionString);
        await _connection.OpenAsync();
        using var transaction = await _connection.BeginTransactionAsync();
        try
        {
            var orderId = await _connection.ExecuteScalarAsync<long>(
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
            // 2. Copy cart items → order items
            var resultitems = await _connection.ExecuteAsync(
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
            // 3. (Optional but recommended) clear cart
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
            await transaction.CommitAsync();
            return resultitems > 0;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// orderdate and customer id arent being updated
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public async Task<bool> UpdateOrder(Order order)
    {
        await using var _connection = new NpgsqlConnection(_connectionString);
        string query = """
            UPDATE orders SET status = @Status WHERE id = @Id;
            """;
        int result = await _connection.ExecuteAsync(
            query,
            new { Status = order.Status, Id = order.Id }
        );

        return result > 0;
    }

    public async Task<bool> DeleteOrder(long id)
    {
        throw new NotImplementedException();
    }
}
