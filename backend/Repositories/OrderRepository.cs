using Backend.Models;
using Dapper;
using Npgsql;

namespace Backend.Repositories;

/// <summary>
/// Deze repository gaat queries uitvoeren met de orders en order_items tabellen in postgreSQL
/// orders bevat "id", "customer_id","order_date","status"
/// order_items bevat "id", "order_id", "product_id", "quantity", "price"
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

    public async Task<Order?> GetOrderByIdAsync(long id, long? userId)
    { //
        // Order? order = await _connection.QueryFirstOrDefaultAsync<Order>(
        //     """
        //     SELECT o.* FROM orders AS o
        //     JOIN order_items AS items ON items.order_id = o.id 
        //     JOIN customers ON customers.id = o.customer_id 
        //     WHERE customers.id = @userId AND o.id = @id;
        //     """,
        //     new { userId, id }
        // );
        // return order;
        // throw new NotImplementedException();
        // https://stackoverflow.com/questions/7508322/how-do-i-map-lists-of-nested-objects-with-dapper
        
        // var sql =
        //     @"SELECT o.*,items.*
        //         FROM orders AS o 
        //         INNER JOIN order_items AS items ON o.Id = items.order_id
        //         WHERE o.id = @id AND o.customer_id = @userId";// add for admin

        var sql =
            @"SELECT o.*,items.*
                FROM orders AS o 
                INNER JOIN order_items AS items ON o.Id = items.order_id";
        var orders = await _connection.QueryAsync<Order, List<OrderItem>, Order?>(
            sql,
            (order, items) =>
            { // userid will make problem fix TODO
                order.Items = items;
                if(order.Id=id && order.CustomerId=userId){return order;}
                return null;
            },
            splitOn: "order_id"
        );//.AsQueryable();
        // var conditions = await _connection.QueryAsync<Order, List<OrderItem>, Order>(
        //     "Select o.*, items.* from orders WHERE o.id = @id AND o.customer_id = @userId and o.Id",
        //     new { Id=id, userid=userId , Orders = orders.Select(m => m.Id).Distinct()}
        // );
        return orders.First();
    }
    public async Task<OrderItem?> GetOrderItemByIdAsync(long id, long? userId)
    { //
        OrderItem? order = await _connection.QueryFirstOrDefaultAsync<OrderItem>(
            """
            SELECT items.* FROM order_items AS items
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
        var sql =
            @"SELECT o.*,items.*
                FROM orders AS o 
                INNER JOIN order_items AS items ON o.Id = items.order_id";

        var orders = await _connection.QueryAsync<Order, List<OrderItem>, Order>(
            sql,
            (order, items) =>
            {
                order.Items = items;
                return order;
            },
            splitOn: "order_id"
        );
        return orders.ToList();

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

    /// <summary>
    /// orderdate and customer id arent being updated
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public async Task<bool> UpdateOrder(Order order)
    {
        // throw new NotImplementedException();
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
