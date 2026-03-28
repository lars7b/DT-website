using Backend.Models;
using Npgsql;

namespace Backend.Services;

// Service met data- en businesslogica voor categorieen.
public sealed class CategoryService : ICategoryService
{
    // Connection string naar de PostgreSQL-database.
    private readonly string _connectionString;

    public CategoryService(IConfiguration configuration)
    {
        // Leest de verbinding uit appsettings of environment variables.
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        // Basisquery: alle categorieen uit de tabel, oplopend op id.
        const string sql = @"SELECT id, name, description
                             FROM categories
                             ORDER BY id ASC";

        // Open een databaseverbinding voor deze aanvraag.
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Voer de query uit en lees alle rijen.
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var categories = new List<Category>();
        while (await reader.ReadAsync(cancellationToken))
        {
            // Zet elke rij uit de database om naar een Category-object.
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Query voor een enkele categorie op basis van id.
        const string sql = @"SELECT id, name, description
                             FROM categories
                             WHERE id = @id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // Parameterized query voorkomt SQL-injection.
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            // Geen match gevonden: laat controller een 404 teruggeven.
            return null;
        }

        return MapCategory(reader);
    }

    // Zet de huidige rij van de DataReader om naar het Category-model.
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