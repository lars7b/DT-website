namespace Backend.Repositories;

using Backend.Models;
using Npgsql;

public class RepositoryBase<T> : IRepository<T>
{
    private readonly string _connectionString;
    private readonly string _table;

    public RepositoryBase(IConfiguration configuration,string table)
    {
        _table = table;
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");
    }

    public virtual async Task<List<T>> GetAll()
    {
        string query = $"SELECT * FROM {_table};";
        List<T> Items = new List<T>();

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        using var command = new NpgsqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Items.Add(default(T));
        }

        return Items;
    }

    public virtual async Task<T?> GetById(long id)
    {
        return default(T);
    }

    public virtual async Task<bool> Add(T entity)
    {
        return false;
    }

    public virtual async Task<bool> Update(T entity)
    {
        return false;
    }

    public virtual async Task<bool> Delete(T entity)
    {
        return false;
    }
}
