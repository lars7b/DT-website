namespace Backend.Repositories;

using Backend.Models;
using Npgsql;

public class CartItemRepository
{
    public CartItemRepository(IConfiguration configuration)
    {
        string connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DB Connection missing");

        db = new NpgsqlConnection(connectionString);
    }
    private readonly NpgsqlConnection db;


}