using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Npgsql;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class CatalogAndFavoritesControllerTests : IntegrationTestBase
{
    public CatalogAndFavoritesControllerTests(CustomApiFactory factory) : base(factory)
    {
        // Authenticeer de client zodat rate limiting (10 req/min per IP voor gasten)
        // de tests niet onderbreekt. De endpoints zelf vereisen geen auth.
        AuthenticateClient(9999, "Customer");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<long> SeedCategoryAsync(string name)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO categories (name, description) VALUES (@Name, 'Test categorie') RETURNING id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("Name", name);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<long> SeedSubcategoryAsync(long categoryId, string name)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO subcategories (category_id, name) VALUES (@CategoryId, @Name) RETURNING id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("CategoryId", categoryId);
        cmd.Parameters.AddWithValue("Name", name);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<long> SeedProductAsync(string name, decimal price)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO products (name, price) VALUES (@Name, @Price) RETURNING id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("Name", name);
        cmd.Parameters.AddWithValue("Price", price);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<int> SeedCustomerAsync(string email)
    {
        int userId = await SeedUserAsync(email, "Customer");
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO customers (user_id, first_name, last_name) VALUES (@UserId, 'Test', 'Klant') RETURNING id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("UserId", userId);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task SeedFavoriteAsync(int customerId, long productId)
    {
        await using var connection = new NpgsqlConnection(_dbConnectionString);
        await connection.OpenAsync();
        var sql = "INSERT INTO favorites (customer_id, product_id) VALUES (@CustomerId, @ProductId);";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("CustomerId", customerId);
        cmd.Parameters.AddWithValue("ProductId", (int)productId);
        await cmd.ExecuteNonQueryAsync();
    }

    // ── CategoriesController ─────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_ShouldReturn200Ok_WhenCategoriesExist()
    {
        // ARRANGE
        await SeedCategoryAsync("Woonkamer");

        // ACT
        var response = await _client.GetAsync("/api/categories");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturn404NotFound_WhenCategoryDoesNotExist()
    {
        // ARRANGE – geen data nodig, id 99999 bestaat niet

        // ACT
        var response = await _client.GetAsync("/api/categories/99999");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ShouldReturn200Ok_WhenCategoryExists()
    {
        // ARRANGE
        long categoryId = await SeedCategoryAsync("Slaapkamer");

        // ACT
        var response = await _client.GetAsync($"/api/categories/{categoryId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        Assert.Equal("Slaapkamer", category.Name);
    }

    // ── SubcategoriesController ──────────────────────────────────────────────

    [Fact]
    public async Task GetSubcategories_ShouldReturn200Ok_WhenSubcategoriesExist()
    {
        // ARRANGE
        long categoryId = await SeedCategoryAsync("Keuken");
        await SeedSubcategoryAsync(categoryId, "Bestek");

        // ACT
        var response = await _client.GetAsync($"/api/subcategories?categoryId={categoryId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var subcategories = await response.Content.ReadFromJsonAsync<List<Subcategory>>();
        Assert.NotNull(subcategories);
        Assert.NotEmpty(subcategories);
    }

    [Fact]
    public async Task GetSubcategoryById_ShouldReturn404NotFound_WhenSubcategoryDoesNotExist()
    {
        // ARRANGE – geen data nodig

        // ACT
        var response = await _client.GetAsync("/api/subcategories/99999");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── FavoritesController ──────────────────────────────────────────────────

    [Fact]
    public async Task GetFavorites_ShouldReturn404NotFound_WhenCustomerDoesNotExist()
    {
        // ARRANGE – klant-id 99999 bestaat niet in de database

        // ACT
        var response = await _client.GetAsync("/api/customers/99999/favorites");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFavorites_ShouldReturn200Ok_WhenCustomerExists()
    {
        // ARRANGE
        int customerId = await SeedCustomerAsync("favorites.klant@test.nl");

        // ACT
        var response = await _client.GetAsync($"/api/customers/{customerId}/favorites");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn201Created_WhenCustomerAndProductExist()
    {
        // ARRANGE
        int customerId = await SeedCustomerAsync("add.favorite@test.nl");
        long productId = await SeedProductAsync("Tafel", 199.99m);

        // ACT
        var response = await _client.PostAsync($"/api/customers/{customerId}/favorites/{productId}", null);

        // ASSERT
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddFavorite_ShouldReturn404NotFound_WhenProductDoesNotExist()
    {
        // ARRANGE
        int customerId = await SeedCustomerAsync("add.favorite.miss@test.nl");

        // ACT
        var response = await _client.PostAsync($"/api/customers/{customerId}/favorites/99999", null);

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFavorite_ShouldReturn204NoContent_WhenFavoriteExists()
    {
        // ARRANGE
        int customerId = await SeedCustomerAsync("remove.favorite@test.nl");
        long productId = await SeedProductAsync("Stoel", 89.99m);
        await SeedFavoriteAsync(customerId, productId);

        // ACT
        var response = await _client.DeleteAsync($"/api/customers/{customerId}/favorites/{productId}");

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // ── OrdersController ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderHistory_ShouldReturn404NotFound_WhenCustomerDoesNotExist()
    {
        // ARRANGE – klant-id 99999 bestaat niet

        // ACT
        var response = await _client.GetAsync("/api/customers/99999/orders");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrderHistory_ShouldReturn200Ok_WhenCustomerExists()
    {
        // ARRANGE
        int customerId = await SeedCustomerAsync("order.history@test.nl");

        // ACT
        var response = await _client.GetAsync($"/api/customers/{customerId}/orders");

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
