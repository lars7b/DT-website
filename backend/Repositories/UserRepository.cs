using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DB Connection missing");
        _logger = logger;
    }

    public async Task<User?> CreateUserWithCustomerAsync(User user)
    {

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            using var userCommand = new NpgsqlCommand(@"
                INSERT INTO users (email, password_hash, role) 
                VALUES (@Email, @PasswordHash, @Role) 
                RETURNING id;", connection, transaction);

            userCommand.Parameters.AddWithValue("@Email", user.Email);
            userCommand.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            userCommand.Parameters.AddWithValue("@Role", user.Role);

            var userId = (int)await userCommand.ExecuteScalarAsync();

            using var customerCommand = new NpgsqlCommand(@"
                INSERT INTO customers (user_id) 
                VALUES (@UserId);", connection, transaction);

            customerCommand.Parameters.AddWithValue("@UserId", userId);

            await customerCommand.ExecuteNonQueryAsync();

            await transaction.CommitAsync();

            user.Id = userId;
            return user;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            await transaction.RollbackAsync();
            return null;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fout bij het aanmaken van user {Email}", user.Email);
            throw;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(@"
                SELECT id, email, password_hash, role
                FROM users
                WHERE email = @Email;", connection);

            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3)
                };
            }
            else return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen user met email {Email}", email);
            throw;
        }
    }
}