using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<CustomApiFactory>
{
    protected readonly HttpClient _client;
    protected readonly string _dbConnectionString;

    protected IntegrationTestBase(CustomApiFactory factory)
    {
        _client = factory.CreateClient();
        
        var config = (IConfiguration)factory.Services.GetService(typeof(IConfiguration))!;
        _dbConnectionString = config.GetConnectionString("DefaultConnection")!;
    }

    protected void AuthenticateClient(int userId, string role)
    {
        var token = GenerateTestJwt(userId, role);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private string GenerateTestJwt(int userId, string role)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected async Task<int> SeedUserAsync(string email, string role)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        
        var sql = "INSERT INTO users (email, password_hash, role) VALUES (@Email, 'dummyhash', @Role) RETURNING id;";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Email", email);
        command.Parameters.AddWithValue("Role", role);
        
        return (int)await command.ExecuteScalarAsync()!;
    }
}