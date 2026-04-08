namespace Backend.Repositories;
using Backend.Models;
using Npgsql;

public class ShoppingCartRepository: RepositoryBase<ShoppingCart>
{
    private static readonly string _table = "shopping_carts";
    private static ShoppingCart _map(NpgsqlDataReader reader)
    {
        return new ShoppingCart
        {
            Id = reader.GetInt64(reader.GetOrdinal("id")),
            CustomerId = reader.GetInt64(reader.GetOrdinal("customer_id")),
        };
    }
    private static readonly string _attributes = "customer_id";
    private static readonly Dictionary<string, string> _reverseMap = new Dictionary<string, string>{{"CustomerId", "customer_id"}};
    public ShoppingCartRepository(IConfiguration configuration)
        : base(configuration, _table, _map, _attributes,_reverseMap) { }

    public async Task<ShoppingCart?> GetByUserIdAsync(long userId)
    {
        string query = " SELECT * FROM shopping_carts WHERE customer_id = @userId LIMIT 1;";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return _map(reader);
        }

        return null;
    }

    public override async Task<bool> Add(ShoppingCart entity)
    {
        var columnList = _reverseMap.Values.ToList();
        var columns = string.Join(", ", columnList);
        var parameters = string.Join(", ", columnList.Select(c => "@" + c));

        string query = $@"
            INSERT INTO {_table} ({columns})
            VALUES ({parameters})
            RETURNING id;
        ";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        foreach (var prop in typeof(ShoppingCart).GetProperties())
        {
            if (!_reverseMap.TryGetValue(prop.Name, out var column))
                continue;

            var value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.AddWithValue("@" + column, value);
        }

        var newId = (long)await command.ExecuteScalarAsync();

        typeof(ShoppingCart).GetProperty("Id")?.SetValue(entity, newId);

        return true;
    }
}