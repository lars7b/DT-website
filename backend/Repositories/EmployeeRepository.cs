using Backend.Models;
using Backend.DTOs;
using Npgsql;
using Dapper;

namespace Backend.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly string _connectionString;
    private readonly ILogger<EmployeeRepository> _logger;

    public EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("DB Connection missing");
        _logger = logger;
    }

    public async Task<EmployeeDto?> GetEmployeeProfileAsync(int employeeId)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            
            var sql = @"
                SELECT u.email, e.first_name AS FirstName, e.last_name AS LastName, e.phone, e.position
                FROM employees e
                JOIN users u ON e.user_id = u.id
                WHERE e.user_id = @UserId;";
                
            return await connection.QuerySingleOrDefaultAsync<EmployeeDto>(sql, new { UserId = employeeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen medewerkersprofiel met id {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> UpdateEmployeeAsync(int employeeId, EmployeeDto request)
    {
        return default;
    }

    public async Task<CustomerAdminDto?> GetCustomerByEmailAsync(string email)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT u.id AS UserId, u.email, c.first_name AS FirstName, c.last_name AS LastName, c.phone, c.address
                FROM users u
                LEFT JOIN customers c ON u.id = c.user_id
                WHERE LOWER(u.email) = LOWER(@Email);";

            return await connection.QuerySingleOrDefaultAsync<CustomerAdminDto>(sql, new { Email = email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen klant met e-mail {Email}", email);
            throw;
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT o.id, o.order_date AS OrderDate, o.status
                FROM orders o
                JOIN customers c ON o.customer_id = c.id
                WHERE c.user_id = @UserId
                ORDER BY o.order_date DESC;";

            return await connection.QueryAsync<OrderDto>(sql, new { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen orders voor user met id {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> CreateEmployeeAsync(CreateEmployeeDto employeeDetails, string passwordHash)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var sqlUser = @"
                    INSERT INTO users (email, password_hash, role) 
                    VALUES (@Email, @PasswordHash, 'Employee') 
                    RETURNING id;";

                var userId = await connection.ExecuteScalarAsync<int>(sqlUser, new 
                { 
                    Email = employeeDetails.Email, 
                    PasswordHash = passwordHash 
                }, transaction);

                var sqlEmployee = @"
                    INSERT INTO employees (user_id, first_name, last_name, phone, position) 
                    VALUES (@UserId, @FirstName, @LastName, @Phone, @Position);";

                await connection.ExecuteAsync(sqlEmployee, new 
                { 
                    UserId = userId, 
                    FirstName = employeeDetails.FirstName, 
                    LastName = employeeDetails.LastName, 
                    Phone = employeeDetails.Phone,
                    Position = employeeDetails.Position
                }, transaction);

                await transaction.CommitAsync();
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij het aanmaken van medewerker met e-mail {Email}", employeeDetails.Email);
            throw;
        }
    }
}
