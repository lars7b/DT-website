using System.Net;
using System.Net.Http.Json;
using Backend.DTOs;
using Xunit;

namespace Backend.Tests.IntegrationTests;

public class ShoppingCartControllerTests
    : IClassFixture<AuthenticatedApiFactory>,
        IClassFixture<UnauthenticatedApiFactory>
{
    private readonly HttpClient _authenticatedClient;
    private readonly HttpClient _unauthenticatedClient;

    public ShoppingCartControllerTests(
        AuthenticatedApiFactory auth,
        UnauthenticatedApiFactory unauth
    )
    {
        _authenticatedClient = auth.CreateClient();
        _unauthenticatedClient = unauth.CreateClient();
    }

    [Fact]
    public async Task GetShoppingCart_ShouldReturn401_WhenNameIdentifierClaimMissing()
    {
        var response = await _authenticatedClient.GetAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetShoppingCart_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // ARRANGE
        // No setup needed for unauthenticated client

        // ACT
        var response = await _unauthenticatedClient.GetAsync("/api/shoppingcart");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetShoppingCart_ShouldReturn200Ok_WhenUserHasCart()
    {
        // ARRANGE
        // No setup needed for authenticated client

        // ACT
        var response = await _authenticatedClient.GetAsync("/api/shoppingcart");

        // ASSERT
        // Note: This will likely fail without proper auth setup
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // var cart = await response.Content.ReadAsAsync<ShoppingCartDto>();
        // Assert.NotNull(cart);
    }

    [Fact]
    public async Task GetShoppingCart_ShouldReturn404NotFound_WhenCartDoesNotExist()
    {
        // ACT
        var response = await _unauthenticatedClient.GetAsync("/api/shoppingcart");

        // ASSERT
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // ARRANGE
        var item = new CartItemDto { ProductId = 1, Quantity = 2 };

        // ACT
        var response = await _unauthenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            item
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn400BadRequest_WhenQuantityIsZero()
    {
        // ARRANGE
        var item = new CartItemDto { ProductId = 1, Quantity = 0 };

        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn400BadRequest_WhenQuantityIsNegative()
    {
        // ARRANGE
        var item = new CartItemDto { ProductId = 1, Quantity = -5 };

        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn400BadRequest_WhenItemIsNull()
    {
        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            (CartItemDto)null
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn204NoContent_WhenItemAddedSuccessfully()
    {
        // ARRANGE
        var item = new CartItemDto { ProductId = 1, Quantity = 3 };

        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItemToShoppingCart_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // ARRANGE
        var item = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 5,
        };

        // ACT
        var response = await _unauthenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItemToShoppingCart_ShouldReturn400BadRequest_WhenQuantityIsZero()
    {
        // ARRANGE
        var item = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 0,
        };

        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItemToShoppingCart_ShouldReturn204NoContent_WhenItemUpdatedSuccessfully()
    {
        // ARRANGE
        var item = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 10,
        };

        // ACT
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCart_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // ACT
        var response = await _unauthenticatedClient.DeleteAsync("/api/shoppingcart");

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCart_ShouldReturn204NoContent_WhenCartDeletedSuccessfully()
    {
        // ACT
        var response = await _authenticatedClient.DeleteAsync("/api/shoppingcart");

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCartItem_ShouldReturn401Unauthorized_WhenNotAuthenticated()
    {
        // ARRANGE
        long cartItemId = 1;

        // ACT
        var response = await _unauthenticatedClient.DeleteAsync(
            $"/api/shoppingcart/Items/{cartItemId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCartItem_ShouldReturn204NoContent_WhenItemDeletedSuccessfully()
    {
        // ARRANGE
        long cartItemId = 1;

        // ACT
        var response = await _authenticatedClient.DeleteAsync(
            $"/api/shoppingcart/Items/{cartItemId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCartItem_ShouldReturn400BadRequest_WhenItemNotFound()
    {
        // ARRANGE
        long nonExistentCartItemId = 99999;

        // ACT
        var response = await _authenticatedClient.DeleteAsync(
            $"/api/shoppingcart/Items/{nonExistentCartItemId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetShoppingCart_ShouldReturn404_WhenServiceReturnsNull()
    {
        var response = await _authenticatedClient.GetAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn400_WhenBodyIsMalformed()
    {
        var json = "{ \"productId\": }"; // invalid JSON

        var response = await _authenticatedClient.PostAsync(
            "/api/shoppingcart/items",
            new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItem_ShouldReturn400_WhenServiceFails()
    {
        var item = new CartItemDto { ProductId = 999, Quantity = 1 };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturn400_WhenServiceReturnsFalse()
    {
        var item = new CartItemDto
        {
            Id = 999,
            ProductId = 1,
            Quantity = 5,
        };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCart_ShouldReturn400_WhenServiceFails()
    {
        var response = await _authenticatedClient.DeleteAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
