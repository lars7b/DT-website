using System.Net;
using System.Net.Http.Json;
using Backend.DTOs;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class OrderContollerTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;

    public OrderContollerTests(CustomApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/order/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/order");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.PostAsync("/api/order", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var order = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            Status = "Pending",
        };

        var response = await _client.PutAsJsonAsync("/api/order/1", order);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.PutAsync("/api/order/1/cancel", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.DeleteAsync("/api/order/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var item = new OrderItemDto
        {
            OrderId = 1,
            ProductId = 1,
            Quantity = 1,
            Price = 10m,
        };

        var response = await _client.PostAsJsonAsync("/api/order/1/items", item);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RemoveItemFromOrder_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.DeleteAsync("/api/order/items/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_ShouldReturn200_WhenOrderExists()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/order/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        Assert.NotNull(order);
    }

    [Fact]
    public async Task GetOrder_ShouldReturn404_WhenOrderDoesNotExist()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/order/9999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturn200_WithOrdersList()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/order");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderDto>>();
        Assert.NotNull(orders);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn204_WhenSuccessful()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var item = new CartItemDto
        {
            Id = 999,
            ProductId = 1,
            Quantity = 5,
        };
        var itemrequest = await client.PostAsJsonAsync("/api/cart/items", item);
        var response = await client.PostAsync("/api/order", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ShouldReturn400_WhenServiceFails()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var delete_response = await client.DeleteAsync("api/shoppingcart");
        var response = await client.PostAsync("/api/order", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturn204_WhenAdminUpdatesOrder()
    {
        var factory = new AdminApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var order = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            Status = "Processing",
        };

        var response = await client.PutAsJsonAsync("/api/order/1", order);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateOrder_ShouldReturn403_WhenUserIsNotAdmin()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var order = new OrderDto
        {
            Id = 1,
            CustomerId = 1,
            Status = "Processing",
        };

        var response = await client.PutAsJsonAsync("/api/order/1", order);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_ShouldReturn200_WhenSuccessful()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.PutAsync("/api/order/1/cancel", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CancelOrder_ShouldReturn400_WhenCancellationFails()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.PutAsync("/api/order/9999999/cancel", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturn204_WhenAdminDeletes()
    {
        var factory = new AdminApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/order/1");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_ShouldReturn403_WhenNotAdmin()
    {
        var factory = new AuthenticatedApiFactory();
        await factory.InitializeAsync();
        await factory.SeedData();
        var client = factory.CreateClient();
        var response = await client.DeleteAsync("/api/order/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
