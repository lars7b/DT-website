using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

/// Basisklasse voor alle integratietesten
/// Laat nieuwe testklassen overerven van IntegrationTestBase in plaats van IClassFixture<CustomApiFactory>
/// Dit centraliseert boilerplate code voor authenticatie en database-setup

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

    /// Gebruik deze methode in de ARRANGE-fase van een test om de _client te voorzien van een geldig JWT-token
    /// Alle API-aanroepen in de ACT-fase worden uitgevoerd als deze specifieke gebruiker
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

    /// Gebruik deze methode in de ARRANGE-fase om snel testgebruikers aan de database toe te voegen
    /// Als je iets anders wilt seeden, zoals een product of bestelling, kun je deze methode als voorbeeld gebruiken
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