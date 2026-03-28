using System.Text;
using Backend.Models;
using Npgsql;

namespace Backend.Services;

// Contains the product business/data logic.
public sealed class ProductService : IProductService
{
    private readonly string _connectionString;

    public ProductService(IConfiguration configuration)
    {
        // Reads the database connection string from appsettings or environment variables.
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        CancellationToken cancellationToken = default)
    {
        // Base query. We append filter conditions only when query parameters are provided.
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

        // Open a PostgreSQL connection for this request.
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ILIKE enables case-insensitive text search in PostgreSQL.
            sql.Append(" AND (p.name ILIKE @search OR p.description ILIKE @search)");
            command.Parameters.AddWithValue("search", $"%{search.Trim()}%");
        }

        if (categoryId.HasValue)
        {
            // Filter to a specific category when categoryId is supplied.
            sql.Append(" AND p.category_id = @categoryId");
            command.Parameters.AddWithValue("categoryId", categoryId.Value);
        }

        if (subcategoryId.HasValue)
        {
            // Filter to a specific subcategory when subcategoryId is supplied.
            sql.Append(" AND p.subcategory_id = @subcategoryId");
            command.Parameters.AddWithValue("subcategoryId", subcategoryId.Value);
        }

        // Stable ordering makes API output predictable.
        sql.Append(" ORDER BY p.id ASC");
        command.CommandText = sql.ToString();

        var products = new List<Product>();

        // Execute query and map each row to a Product object.
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Query one product by id.
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
            // Returning null lets the controller respond with 404 Not Found.
            return null;
        }

        return MapProduct(reader);
    }

    // Converts the current database row into a Product model.
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