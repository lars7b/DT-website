using Backend.Models;
using Backend.DTOs;
using Npgsql;
using Dapper;

namespace Backend.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(IConfiguration configuration, ILogger<CustomerRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DB Connection missing");
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerAsync(int userId)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlCustomer = @"
                SELECT Id, user_id, first_name, last_name, phone, address
                FROM customers
                WHERE user_id = @userId;";
            return await connection.QuerySingleOrDefaultAsync<Customer>(sqlCustomer, new { userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen customer met id {userId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateCustomerAsync(int userId, CustomerDto customerDetails)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            var sqlUpdate = @"
                UPDATE customers
                SET
                    first_name = COALESCE(@FirstName, first_name),
                    last_name = COALESCE(@LastName, last_name),
                    phone = COALESCE(@Phone, phone),
                    address = COALESCE(@Address, address)
                WHERE user_id = @UserId;";

            var parameters = new
            {
                UserId = userId,
                FirstName = customerDetails.FirstName,
                LastName = customerDetails.LastName,
                Phone = customerDetails.Phone,
                Address = customerDetails.Address
            };

            var rowsAffected = await connection.ExecuteAsync(sqlUpdate, parameters);

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij updaten customer met id {userId}", userId);
            throw;
        }
    }

    public async Task<bool> DeleteCustomerAsync(int userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var sqlCustomer = @"
            UPDATE customers 
            SET first_name = 'Deleted', 
                last_name = 'User', 
                phone = NULL, 
                address = NULL 
            WHERE user_id = @UserId;";

            await connection.ExecuteAsync(sqlCustomer, new { UserId = userId }, transaction);

            var fakeEmail = $"deleted_{Guid.NewGuid()}@anoniem.nl";
            var sqlUser = @"
                UPDATE users 
                SET email = @FakeEmail, 
                    password_hash = 'DELETED', 
                    role = 'Deleted' 
                WHERE id = @UserId;";
            
            await connection.ExecuteAsync(sqlUser, new { UserId = userId, FakeEmail = fakeEmail }, transaction);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Fout bij deleten customer met id {userId}", userId);
            throw;
        }
    }
}
