using System.Threading.Tasks;
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
    protected virtual bool UseFakeAdmin => false;
    protected virtual bool SeedDatabase => false;
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("test_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    public CustomApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "JWT_SECRET_KEY",
            "A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"
        );
        Environment.SetEnvironmentVariable(
            "JwtSettings__Key",
            "A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"
        );
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "TestAudience");
    }

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

        await InitializeDatabaseAsync();
        if (SeedDatabase)
        {
            await SeedData();
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        var connectionString = _dbContainer.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var basePath = Path.Combine(AppContext.BaseDirectory, "database");

        var schemaPath = Path.Combine(basePath, "01-schema.sql");
        // var seedPath = Path.Combine(basePath, "02-data.sql");
        // var testseedPath = Path.GetFullPath(
        //     Path.Combine(
        //         AppContext.BaseDirectory,
        //         "..",
        //         "..",
        //         "..",
        //         "..",
        //         "backend.Tests",
        //         "testdata.sql"
        //     )
        // );

        if (!File.Exists(schemaPath))
            throw new FileNotFoundException("Schema file not found", schemaPath);

        // if (!File.Exists(seedPath))
        // throw new FileNotFoundException("Seed file not found", seedPath);

        // if (!File.Exists(testseedPath))
        //     throw new FileNotFoundException("Schema file not found", testseedPath);

        string sql_scripts = await File.ReadAllTextAsync(schemaPath);

        // Voer het scripts uit
        await using var command = new NpgsqlCommand(sql_scripts, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task SeedData()
    {
        // delete everything to ensure no errors
        
        //seed data
        var basePath = Path.Combine(AppContext.BaseDirectory, "database");
        var connectionString = _dbContainer.GetConnectionString();
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        var seedPath = Path.Combine(basePath, "02-data.sql");
        var testseedPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "backend.Tests",
                "testdata.sql"
            )
        );

        if (!File.Exists(seedPath))
            throw new FileNotFoundException("Seed file not found", seedPath);

        if (!File.Exists(testseedPath))
            throw new FileNotFoundException("Schema file not found", testseedPath);

        string seed_scripts = await File.ReadAllTextAsync(seedPath);
        string test_scripts = await File.ReadAllTextAsync(testseedPath);

        var truncate = """
            TRUNCATE TABLE
                users,
                employees,
                customers,
                categories,
                subcategories,
                products,
                shopping_carts,
                cart_items,
                orders,
                order_items,
                order_status_history,
                payments,
                reviews,
                favorites
            RESTART IDENTITY CASCADE;
            """;

        await using var truncate_command = new NpgsqlCommand(truncate, connection);
        await truncate_command.ExecuteNonQueryAsync();

        await using var second_command = new NpgsqlCommand(seed_scripts, connection);
        await second_command.ExecuteNonQueryAsync();

        await using var third_command = new NpgsqlCommand(test_scripts, connection);
        await third_command.ExecuteNonQueryAsync();
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
                        {
                            "ConnectionStrings:DefaultConnection",
                            _dbContainer.GetConnectionString()
                        },
                        {
                            "ConnectionStrings:RedisDefaultConnection",
                            _redisContainer.GetConnectionString()
                        },
                        { "DB_PASSWORD", "dummy_db_password" },
                        { "REDIS_PASSWORD", "dummy_redis_password" },
                        {
                            "JWT_SECRET_KEY",
                            "A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"
                        },
                        {
                            "JwtSettings:Key",
                            "A_Very_Long_Super_Secret_Key_For_Testing_Only_12345!"
                        },
                        { "JwtSettings:Issuer", "TestIssuer" },
                        { "JwtSettings:Audience", "TestAudience" },
                    }
                );
            }
        );
        if (UseFakeAuth && !UseFakeAdmin)
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
        if (UseFakeAuth && UseFakeAdmin)
        {
            builder.ConfigureTestServices(services =>
            {
                services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = "Test";
                        options.DefaultChallengeScheme = "Test";
                    })
                    .AddScheme<AuthenticationSchemeOptions, AdminAuthHandler>("Test", _ => { });
            });
        }
    }

    public new async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
    }
}

public class AuthenticatedApiFactory : CustomApiFactory
{
    protected override bool UseFakeAuth => true;

    protected override bool SeedDatabase => true;
}

public class UnauthenticatedApiFactory : CustomApiFactory
{
    protected override bool UseFakeAuth => false;
    protected override bool SeedDatabase => true;
}

public class AdminApiFactory : CustomApiFactory
{
    protected override bool UseFakeAuth => true;
    protected override bool UseFakeAdmin => true;
    protected override bool SeedDatabase => true;
}
