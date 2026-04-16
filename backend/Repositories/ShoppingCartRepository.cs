namespace Backend.Repositories;
using Backend.Models;
using Npgsql;

public class ShoppingCartRepository{
    private readonly NpgsqlConnection db;

    public ShoppingCartRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        db = new NpgsqlConnection(connectionString);
    }
    public async Task<ShoppingCart?> GetByUserIdAsync(long userId)
    {
        // string query = " SELECT * FROM shopping_carts WHERE customer_id = @userId LIMIT 1;";
        // await using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();

        // await using var command = new NpgsqlCommand(query, connection);
        // command.Parameters.AddWithValue("@userId", userId);

        // await using var reader = await command.ExecuteReaderAsync();

        // if (await reader.ReadAsync())
        // {
        //     return _map(reader);
        // }

        return null;
    }

    public async Task<bool> Add(ShoppingCart entity)
    {
        // var columnList = _reverseMap.Values.ToList();
        // var columns = string.Join(", ", columnList);
        // var parameters = string.Join(", ", columnList.Select(c => "@" + c));

        // string query = $@"
        //     INSERT INTO {_table} ({columns})
        //     VALUES ({parameters})
        //     RETURNING id;
        // ";

        // await using var connection = new NpgsqlConnection(_connectionString);
        // await connection.OpenAsync();

        // await using var command = new NpgsqlCommand(query, connection);

        // foreach (var prop in typeof(ShoppingCart).GetProperties())
        // {
        //     if (!_reverseMap.TryGetValue(prop.Name, out var column))
        //         continue;

        //     var value = prop.GetValue(entity) ?? DBNull.Value;
        //     command.Parameters.AddWithValue("@" + column, value);
        // }

        // var newId = (long)await command.ExecuteScalarAsync();

        // typeof(ShoppingCart).GetProperty("Id")?.SetValue(entity, newId);

        return true;
    }
    public async Task<ShoppingCart?> GetById(long id)
    {
        return null;
    }
    // public async Task<ShoppingCart> 
    public async Task<bool> Update(ShoppingCart cart){return false;}
    public async Task<bool>  Delete(ShoppingCart cart){return false;}
}