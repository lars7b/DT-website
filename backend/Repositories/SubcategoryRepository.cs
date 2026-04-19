using Backend.Models;
using Npgsql;
using System.Text;

namespace Backend.Repositories;

public class SubcategoryRepository
{
    private readonly string _connectionString;

    public SubcategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Subcategory>> GetSubcategoriesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder(
            @"SELECT s.id,
                     s.category_id,
                     s.name,
                     s.description,
                     c.name AS category_name
              FROM subcategories s
              LEFT JOIN categories c ON c.id = s.category_id
              WHERE 1 = 1");

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        if (categoryId.HasValue)
        {
            sql.Append(" AND s.category_id = @categoryId");
            command.Parameters.AddWithValue("categoryId", categoryId.Value);
        }

        sql.Append(" ORDER BY s.id ASC");
        command.CommandText = sql.ToString();

        var subcategories = new List<Subcategory>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            subcategories.Add(MapSubcategory(reader));
        }

        return subcategories;
    }

    public async Task<Subcategory?> GetSubcategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT s.id,
                                    s.category_id,
                                    s.name,
                                    s.description,
                                    c.name AS category_name
                             FROM subcategories s
                             LEFT JOIN categories c ON c.id = s.category_id
                             WHERE s.id = @id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapSubcategory(reader);
    }

    private static Subcategory MapSubcategory(NpgsqlDataReader reader)
    {
        return new Subcategory
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("category_id")),
            CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name"))
                ? null
                : reader.GetString(reader.GetOrdinal("category_name")),
            Name = reader.IsDBNull(reader.GetOrdinal("name"))
                ? null
                : reader.GetString(reader.GetOrdinal("name")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description"))
        };
    }
}