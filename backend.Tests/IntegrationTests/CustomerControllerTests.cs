using System.Net;
using System.Net.Http.Json;
using Backend.DTOs;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class CustomerControllerTests : IntegrationTestBase
{
    public CustomerControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetMyProfile_ShouldReturn200Ok_WhenCustomerExists()
    {
        // ARRANGE
        int userId = await SeedUserAsync("customer1@test.nl", "Customer");
        
        // Seed klant
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO customers (user_id, first_name, last_name) VALUES (@UserId, 'Test', 'Klant');";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("UserId", userId);
        await command.ExecuteNonQueryAsync();

        // JWT
        AuthenticateClient(userId, "Customer");

        // ACT
        var response = await _client.GetAsync("/api/customer/me");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<CustomerDto>();
        Assert.NotNull(profile);
        Assert.Equal("Test", profile.FirstName);
    }

    [Fact]
    public async Task GetMyProfile_ShouldReturn401Unauthorized_WhenNoTokenIsProvided()
    {
        // ARRANGE
        _client.DefaultRequestHeaders.Authorization = null;

        // ACT
        var response = await _client.GetAsync("/api/customer/me");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}