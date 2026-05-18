using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class CategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id, name, description
                             FROM categories
                             ORDER BY id ASC";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var categories = new List<Category>();
        while (await reader.ReadAsync(cancellationToken))
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id, name, description
                             FROM categories
                             WHERE id = @id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapCategory(reader);
    }

    private static Category MapCategory(NpgsqlDataReader reader)
    {
        return new Category
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.IsDBNull(reader.GetOrdinal("name"))
                ? null
                : reader.GetString(reader.GetOrdinal("name")),
            Description = reader.IsDBNull(reader.GetOrdinal("description"))
                ? null
                : reader.GetString(reader.GetOrdinal("description"))
        };
    }
}