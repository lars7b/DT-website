using System.Text;
using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing."
            );
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? sort,
        int? offset,
        int? limit,
        CancellationToken cancellationToken = default
    )
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
          WHERE 1 = 1"
        );

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

        if (minPrice.HasValue)
        {
            sql.Append(" AND p.price >= @minPrice");
            command.Parameters.AddWithValue("minPrice", minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            sql.Append(" AND p.price <= @maxPrice");
            command.Parameters.AddWithValue("maxPrice", maxPrice.Value);
        }

        if (sort == "price_asc")
            sql.Append(" ORDER BY p.price ASC");
        else if (sort == "price_desc")
            sql.Append(" ORDER BY p.price DESC");
        else
            sql.Append(" ORDER BY p.id ASC");

        if (limit.HasValue)
        {
            sql.Append(" LIMIT @limit");
            command.Parameters.AddWithValue("limit", limit.Value);
        }

        if (offset.HasValue)
        {
            sql.Append(" OFFSET @offset");
            command.Parameters.AddWithValue("offset", offset.Value);
        }

        command.CommandText = sql.ToString();

        var products = new List<Product>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var ordinals = GetOrdinals(reader);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader, ordinals));
        }

        return products;
    }

    public async Task<Product?> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        const string sql =
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

        var ordinals = GetOrdinals(reader);
        return MapProduct(reader, ordinals);
    }

    public async Task<Product> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO products (name, description, price, category_id, subcategory_id)
                             VALUES (@name, @description, @price, @categoryId, @subcategoryId)
                             RETURNING id,
                                       name,
                                       description,
                                       price,
                                       category_id,
                                       subcategory_id,
                                       NULL::text AS category_name,
                                       NULL::text AS subcategory_name";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("name", product.Name);
        command.Parameters.AddWithValue("description", (object?)product.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("price", product.Price);
        command.Parameters.AddWithValue("categoryId", (object?)product.CategoryId ?? DBNull.Value);
        command.Parameters.AddWithValue("subcategoryId", (object?)product.SubcategoryId ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        var ordinals = GetOrdinals(reader);
        return MapProduct(reader, ordinals);
    }

    private static ProductOrdinals GetOrdinals(NpgsqlDataReader reader)
    {
        return new ProductOrdinals(
            reader.GetOrdinal("id"),
            reader.GetOrdinal("name"),
            reader.GetOrdinal("description"),
            reader.GetOrdinal("price"),
            reader.GetOrdinal("category_id"),
            reader.GetOrdinal("subcategory_id"),
            reader.GetOrdinal("category_name"),
            reader.GetOrdinal("subcategory_name")
        );
    }

    private static Product MapProduct(NpgsqlDataReader reader, ProductOrdinals ordinals)
    {
        return new Product
        {
            Id = reader.GetInt32(ordinals.Id),
            Name = reader.GetString(ordinals.Name),
            Description = reader.IsDBNull(ordinals.Description)
                ? null
                : reader.GetString(ordinals.Description),
            Price = reader.GetDecimal(ordinals.Price),
            CategoryId = reader.IsDBNull(ordinals.CategoryId)
                ? null
                : reader.GetInt32(ordinals.CategoryId),
            SubcategoryId = reader.IsDBNull(ordinals.SubcategoryId)
                ? null
                : reader.GetInt32(ordinals.SubcategoryId),
            CategoryName = reader.IsDBNull(ordinals.CategoryName)
                ? null
                : reader.GetString(ordinals.CategoryName),
            SubcategoryName = reader.IsDBNull(ordinals.SubcategoryName)
                ? null
                : reader.GetString(ordinals.SubcategoryName),
        };
    }

    private record ProductOrdinals(
        int Id,
        int Name,
        int Description,
        int Price,
        int CategoryId,
        int SubcategoryId,
        int CategoryName,
        int SubcategoryName
    );
}
