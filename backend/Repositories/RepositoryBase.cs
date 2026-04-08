namespace Backend.Repositories;

using Backend.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

public class RepositoryBase<T>
    where T : IModel
{
    protected readonly string _connectionString;
    private readonly string _table;
    private readonly Func<NpgsqlDataReader, T> _map;
    private readonly string _attributes;
    private readonly Dictionary<string, string> _reverseMap;

    public RepositoryBase(
        IConfiguration configuration,
        string table,
        Func<NpgsqlDataReader, T> map,
        string attributes,
        Dictionary<string, string> reverseMap
    )
    {
        _table = table;
        _map = map;
        _attributes = attributes;
        _reverseMap = reverseMap;

        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");
    }

    protected void GetReader()
    {
        throw new NotImplementedException();
    }

    public async Task<List<T>> GetAll()
    {
        var items = new List<T>();
        string query = $"SELECT * FROM {_table};";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(_map(reader));
        }

        return items;
    }

    public async Task<T?> GetById(long id)
    {
        string query = $"SELECT * FROM {_table} WHERE id = @id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return _map(reader);
        }

        return default(T);
    }

    public virtual async Task<bool> Add(T entity)
    {
        var columns = _attributes;

        var paramNames = _attributes.Split(", ").Select(a => "@" + a);

        var values = string.Join(", ", paramNames);

        string query = $"INSERT INTO {_table} ({columns}) VALUES ({values});";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        foreach (var prop in typeof(T).GetProperties())
        {
            if (!_reverseMap.TryGetValue(prop.Name, out var column))
                continue;

            var value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.AddWithValue("@" + column, value);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> Update(T entity)
    {
        var setClause = string.Join(", ", _attributes.Split(", ").Select(a => $"{a} = @{a}"));

        string query = $"UPDATE {_table} SET {setClause} WHERE id = @id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);

        foreach (var prop in typeof(T).GetProperties())
        {
            if (!_reverseMap.TryGetValue(prop.Name, out var column))
                continue;

            var value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.AddWithValue("@" + column, value);
        }

        command.Parameters.AddWithValue("@id", entity.Id);

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> Delete(T? entity)
    {
        if (entity == null)
        {
            return false;
        }

        var id = typeof(T).GetProperty("Id").GetValue(entity);

        if (id == null)
            throw new InvalidOperationException("Id cannot be null");

        string query = $"DELETE FROM {_table} WHERE id = @id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteById(long id)
    {
        string query = $"DELETE FROM {_table} WHERE id = @id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<bool> UpdateById(long id, T entity)
    {
        var properties = typeof(T).GetProperties().Where(p => p.Name.ToLower() != "id");

        var setClause = string.Join(
            ", ",
            properties.Select(p => $"{p.Name.ToLower()} = @{p.Name.ToLower()}")
        );

        string query = $"UPDATE {_table} SET {setClause} WHERE id = @id;";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", id);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.AddWithValue("@" + prop.Name.ToLower(), value);
        }

        var rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
