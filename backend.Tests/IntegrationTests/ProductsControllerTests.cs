using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(CustomApiFactory factory) : base(factory) { }

    [Fact]
    public async Task GetProducts_ShouldReturn200Ok()
    {
        // ARRANGE
        await SeedProductAsync("Test Bank", 899.00m);

        // ACT
        var response = await _client.GetAsync("/api/Products");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();

        Assert.NotNull(products);
        Assert.NotEmpty(products);
    }

    [Fact]
    public async Task GetProductById_ShouldReturn200Ok_WhenProductExists()
    {
        // ARRANGE
        var productId = await SeedProductAsync("Test Stoel", 149.00m);

        // ACT
        var response = await _client.GetAsync($"/api/Products/{productId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<Product>();

        Assert.NotNull(product);
        Assert.Equal(productId, product.Id);
        Assert.Equal("Test Stoel", product.Name);
        Assert.Equal(149.00m, product.Price);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturn201Created_WhenValidProductIsProvided()
    {
        // ARRANGE
        var request = new Product
        {
            Name = "Nieuwe Tafel",
            Description = "Een mooie nieuwe tafel",
            Price = 249.99m
        };

        // ACT
        var response = await _client.PostAsJsonAsync("/api/Products", request);

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();

        Assert.NotNull(createdProduct);
        Assert.True(createdProduct.Id > 0);
        Assert.Equal("Nieuwe Tafel", createdProduct.Name);
        Assert.Equal(249.99m, createdProduct.Price);
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
}