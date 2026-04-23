using Backend.Models;
using Npgsql;
using System.Text;

namespace Backend.Repositories;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder(
            @"SELECT p.id,
                     p.name,
                     p.description,
                     p.price,
                     p.category_id,
                     p.subcategory_id,
                     c.name AS category_name,
                     s.name AS subcategory_name
              FROM products p
              LEFT JOIN categories c ON c.id = p.category_id
              LEFT JOIN subcategories s ON s.id = p.subcategory_id
              WHERE 1 = 1");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(search))
        {
            sql.Append(" AND (p.name ILIKE @search OR p.description ILIKE @search)");
            command.Parameters.AddWithValue("search", $"%{search}%");
        }

        if (categoryId.HasValue)
        {
            sql.Append(" AND p.category_id = @categoryId");
            command.Parameters.AddWithValue("categoryId", categoryId.Value);
        }

        if (subcategoryId.HasValue)
        {
            sql.Append(" AND p.subcategory_id = @subcategoryId");
            command.Parameters.AddWithValue("subcategoryId", subcategoryId.Value);
        }

        sql.Append(" ORDER BY p.id ASC");
        command.CommandText = sql.ToString();

        var products = new List<Product>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT p.id,
                                    p.name,
                                    p.description,
                                    p.price,
                                    p.category_id,
                                    p.subcategory_id,
                                    c.name AS category_name,
                                    s.name AS subcategory_name
                             FROM products p
                             LEFT JOIN categories c ON c.id = p.category_id
                             LEFT JOIN subcategories s ON s.id = p.subcategory_id
                             WHERE p.id = @id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapProduct(reader);
    }

    private static Product MapProduct(NpgsqlDataReader reader)
    {
        return new Product
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description")),
            Price = reader.GetDecimal(reader.GetOrdinal("price")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("category_id")),
            SubcategoryId = reader.IsDBNull(reader.GetOrdinal("subcategory_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("subcategory_id")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("category_name")),
            SubcategoryName = reader.IsDBNull(reader.GetOrdinal("subcategory_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("subcategory_name"))
        };
    }
}