using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class CustomApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected virtual bool UseFakeAuth => false;
    private  readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7")
        .Build();

    public CustomApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "JwtSettings__Key",
            "A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"
        );
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");
        _dbContainer.StartAsync().GetAwaiter().GetResult();
    _redisContainer.StartAsync().GetAwaiter().GetResult();
    }

    public async ValueTask InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
        await InitializeDatabaseAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        var connectionString = _dbContainer.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        string sql_scripts = await File.ReadAllTextAsync("..\\..\\..\\..\\database\\01-schema.sql");
        // var sql = await File.ReadAllTextAsync("01-schema.sql");

        // Voer het script uit
        await using var command = new NpgsqlCommand(sql_scripts, connection);
        await command.ExecuteNonQueryAsync();

        //seed data
        string seed_scripts = await File.ReadAllTextAsync("..\\..\\..\\..\\database\\02-data.sql");

        await using var second_command = new NpgsqlCommand(seed_scripts, connection);
        await second_command.ExecuteNonQueryAsync();
    }

    // Overschrijf de config, pak de connectionstring van test_db
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            _dbContainer.GetConnectionString(),

                        ["ConnectionStrings:RedisDefaultConnection"] =
                            _redisContainer.GetConnectionString(),
                    }
                );
            }
        );

        if (UseFakeAuth)
        {
            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, AuthHandler>("Test", _ => { });
            });
        }
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}

public class AuthenticatedApiFactory : CustomApiFactory
{
    protected override bool UseFakeAuth => true;
}


public class UnauthenticatedApiFactory : CustomApiFactory
{
    protected override bool UseFakeAuth => false;
}