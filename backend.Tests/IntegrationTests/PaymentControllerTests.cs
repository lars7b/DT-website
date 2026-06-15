using System;
using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class PaymentControllerTests : IClassFixture<CustomApiFactory>
{
    private readonly HttpClient _client;

    public PaymentControllerTests(CustomApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPaymentById_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/payment/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllPayments_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/payment");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatePayment_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var payment = new Payment
        {
            Id = 1,
            OrderId = 1,
            Amount = 10.00m,
            PaymentMethod = "CreditCard",
            Status = "Pending",
            PaymentDate = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/payment", payment);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePayment_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var payment = new Payment
        {
            Id = 1,
            OrderId = 1,
            Amount = 10.00m,
            PaymentMethod = "CreditCard",
            Status = "Pending",
            PaymentDate = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync("/api/payment/1", payment);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeletePayment_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        var response = await _client.DeleteAsync("/api/payment/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
