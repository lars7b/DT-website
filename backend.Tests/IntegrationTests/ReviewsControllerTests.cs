using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class ReviewsControllerTests : IntegrationTestBase
{
    public ReviewsControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetReviewsByProductId_ShouldReturn200Ok()
    {
        // ARRANGE
        var productId = await SeedProductAsync("Test Bank", 899.00m);
        var customerId = await SeedCustomerAsync("reviewcustomer1@test.nl");

        await SeedReviewAsync(customerId, productId, 5, "Goed product.");

        // ACT
        var response = await _client.GetAsync($"/api/reviews/product/{productId}");

        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateReview_ShouldReturn201Created_WhenValidReviewIsProvided()
    {
        // ARRANGE
        var productId = await SeedProductAsync("Test Stoel", 149.00m);
        var customerId = await SeedCustomerAsync("reviewcustomer2@test.nl");

        var request = new Review
        {
            CustomerId = customerId,
            ProductId = productId,
            Rating = 5,
            Comment = "Heel mooi product."
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/reviews", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdReview = await response.Content.ReadFromJsonAsync<Review>();

        Assert.NotNull(createdReview);
        Assert.True(createdReview.Id > 0);
        Assert.Equal(customerId, createdReview.CustomerId);
        Assert.Equal(productId, createdReview.ProductId);
        Assert.Equal(5, createdReview.Rating);
        Assert.Equal("Heel mooi product.", createdReview.Comment);
    }

    [Fact]
    public async Task CreateReview_ShouldReturn400BadRequest_WhenRatingIsInvalid()
    {
        // ARRANGE
        var productId = await SeedProductAsync("Test Tafel", 249.00m);
        var customerId = await SeedCustomerAsync("reviewcustomer3@test.nl");

        var request = new Review
        {
            CustomerId = customerId,
            ProductId = productId,
            Rating = 6,
            Comment = "Deze rating is ongeldig."
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/reviews", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<int> SeedCustomerAsync(string email)
    {
        var userId = await SeedUserAsync(email, "Customer");

        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO customers (user_id, first_name, last_name)
            VALUES (@UserId, 'Test', 'Customer')
            RETURNING id;
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("UserId", userId);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<int> SeedProductAsync(string name, decimal price)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO products (name, description, price)
            VALUES (@Name, 'Test beschrijving', @Price)
            RETURNING id;
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Name", name);
        command.Parameters.AddWithValue("Price", price);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private async Task<int> SeedReviewAsync(int customerId, int productId, int rating, string comment)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO reviews (customer_id, product_id, rating, comment)
            VALUES (@CustomerId, @ProductId, @Rating, @Comment)
            RETURNING id;
        ";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("CustomerId", customerId);
        command.Parameters.AddWithValue("ProductId", productId);
        command.Parameters.AddWithValue("Rating", rating);
        command.Parameters.AddWithValue("Comment", comment);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
}