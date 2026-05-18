using Backend.Models;
using Backend.DTOs;
using Npgsql;
using Dapper;

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
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var sqlUser = @"
                INSERT INTO users (email, password_hash, role) 
                VALUES (@Email, @PasswordHash, @Role) 
                RETURNING id;";

            var userId = await connection.ExecuteScalarAsync<int>(sqlUser, user, transaction);

            var sqlCustomer = @"
                INSERT INTO customers (user_id) 
                VALUES (@UserId);";

            await connection.ExecuteAsync(sqlCustomer, new { UserId = userId }, transaction);

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
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlUser = @"
                SELECT id, email, password_hash, role
                FROM users
                WHERE email = @Email;";
            return await connection.QuerySingleOrDefaultAsync<User>(sqlUser, new { email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen user met email {Email}", email);
            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlUser = @"
                SELECT id, email, password_hash, role
                FROM users
                WHERE id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<User>(sqlUser, new { userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen user met id {userId}", userId);
            throw;
        }
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPasswordHash)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlPassword = @"
                UPDATE users
                SET password_hash = @newPasswordHash
                WHERE id = @userId;";
            int rowsAffected = await connection.ExecuteAsync(sqlPassword, new { newPasswordHash, userId });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij updaten wachtwoord voor user {userId}", userId);
            throw;
        }
    }

    public async Task<bool> ChangeEmailAsync(int userId, string newEmail)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlEmail = @"
                UPDATE users
                SET email = @newEmail
                WHERE id = @userId;";
            int rowsAffected = await connection.ExecuteAsync(sqlEmail, new { newEmail, userId });

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij updaten email voor user {userId}", userId);
            throw;
        }
    }
}