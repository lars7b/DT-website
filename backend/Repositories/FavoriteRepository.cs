using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class FavoriteRepository
{
    private readonly string _connectionString;

    public FavoriteRepository(IConfiguration configuration)
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

    public async Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT 1
                             FROM products
                             WHERE id = @productId";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("productId", productId);

        return await command.ExecuteScalarAsync(cancellationToken) is not null;
    }

    public async Task<IReadOnlyList<FavoriteProduct>> GetFavoritesByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT f.id,
                                    f.customer_id,
                                    f.product_id,
                                    f.created_at,
                                    p.name AS product_name,
                                    p.description AS product_description,
                                    p.price,
                                    p.category_id,
                                    p.subcategory_id,
                                    c.name AS category_name,
                                    s.name AS subcategory_name
                             FROM favorites f
                             INNER JOIN products p ON p.id = f.product_id
                             LEFT JOIN categories c ON c.id = p.category_id
                             LEFT JOIN subcategories s ON s.id = p.subcategory_id
                             WHERE f.customer_id = @customerId
                             ORDER BY f.created_at DESC, f.id DESC";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var favorites = new List<FavoriteProduct>();

        while (await reader.ReadAsync(cancellationToken))
        {
            favorites.Add(MapFavoriteProduct(reader));
        }

        return favorites;
    }

    public async Task<bool> AddFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO favorites (customer_id, product_id)
                             VALUES (@customerId, @productId)
                             ON CONFLICT (customer_id, product_id) DO NOTHING";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("productId", productId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> RemoveFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        const string sql = @"DELETE FROM favorites
                             WHERE customer_id = @customerId
                               AND product_id = @productId";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", customerId);
        command.Parameters.AddWithValue("productId", productId);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private static FavoriteProduct MapFavoriteProduct(NpgsqlDataReader reader)
    {
        return new FavoriteProduct
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
            ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
            Name = reader.IsDBNull(reader.GetOrdinal("product_name"))
                ? string.Empty
                : reader.GetString(reader.GetOrdinal("product_name")),
            Description = reader.IsDBNull(reader.GetOrdinal("product_description"))
                ? null
                : reader.GetString(reader.GetOrdinal("product_description")),
            Price = reader.GetDecimal(reader.GetOrdinal("price")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("category_id")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("category_name")),
            SubcategoryId = reader.IsDBNull(reader.GetOrdinal("subcategory_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("subcategory_id")),
            SubcategoryName = reader.IsDBNull(reader.GetOrdinal("subcategory_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("subcategory_name"))
        };
    }
}