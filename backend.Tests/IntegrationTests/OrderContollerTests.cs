using System.Net;
using System.Net.Http.Json;
using Xunit;
using Backend.DTOs;

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
            Status = "Pending"
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
            Price = 10m
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
}
