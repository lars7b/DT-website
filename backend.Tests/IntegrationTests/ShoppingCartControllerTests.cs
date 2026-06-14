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
        // ACT: ensure doesnt exist by deleting and get
        var delete_response = await _authenticatedClient.DeleteAsync("/api/shoppingcart");

        var response = await _authenticatedClient.GetAsync("/api/shoppingcart");

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
        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items",(CartItemDto)null);

        // ASSERT
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddItemToShoppingCart_ShouldReturn204NoContent_WhenItemAddedSuccessfully()
    {
        // ARRANGE
        await _authenticatedClient.DeleteAsync("/api/shoppingcart");
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
        var updated_item = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 20,
        };

        // ACT
        var response = await _unauthenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            item
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // ACT
        var update_response = await _unauthenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            updated_item
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.Unauthorized, update_response.StatusCode);
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
    public async Task UpdateItemToShoppingCart_ShouldUpdateQuantity()
    {
        // Arrange
        await _authenticatedClient.DeleteAsync("/api/shoppingcart");
        var postitem = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 5,
        };

        var updatedItem = new CartItemDto
        {
            Id = 1,
            ProductId = 1,
            Quantity = 20,
        };

        // Create
        var createResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            postitem
        );

        Assert.Equal(HttpStatusCode.NoContent, createResponse.StatusCode);

        // Update
        var updateResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            updatedItem
        );

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Get
        var getResponse = await _authenticatedClient.GetAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var items = await _authenticatedClient.GetFromJsonAsync<ShoppingCartDto>("/api/shoppingcart");

        var item = items.Items.Single(x => x.ProductId == 1);

        Assert.Equal(25, item.Quantity);
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
        var itemToAdd = new CartItemDto { ProductId = 1, Quantity = 2 };

        // 1. Add item first (ensures it exists)
        var addResponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            itemToAdd
        );

        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        // 2. Get cart to retrieve real item id
        var cart = await _authenticatedClient.GetFromJsonAsync<ShoppingCartDto>(
            "/api/shoppingcart"
        );

        Assert.NotNull(cart);
        Assert.NotEmpty(cart.Items);

        var insertedItem = cart.Items.FirstOrDefault(x => x.ProductId == itemToAdd.ProductId);

        Assert.NotNull(insertedItem);

        long cartItemId = insertedItem.Id;

        // ACT
        var response = await _authenticatedClient.DeleteAsync(
            $"/api/shoppingcart/items/{cartItemId}"
        );

        // ASSERT
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // VERIFY DELETION
        var getResponse = await _authenticatedClient.GetAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
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
        var response = await _authenticatedClient.GetAsync( // GetFromJsonAsync
            "/api/shoppingcart"
        );
        // Assert.Equal(0, response.Items.Count);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var delete = await _authenticatedClient.DeleteAsync("/api/shoppingcart");

        var second_response = await _authenticatedClient.GetAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.NotFound, second_response.StatusCode);
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
    public async Task AddItem_ShouldReturn400_WhenServiceCantFindProduct()
    {
        var item = new CartItemDto { ProductId = 99999999, Quantity = 1 };

        var response = await _authenticatedClient.PostAsJsonAsync("/api/shoppingcart/items", item);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturn400_WhenServiceCantFindProduct()
    {
        var item = new CartItemDto { ProductId = 9999999999, Quantity = 5 };

        var addresponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            item
        );

        Assert.Equal(HttpStatusCode.BadRequest, addresponse.StatusCode);

        var itemupdate = new CartItemDto { ProductId = 9999999999, Quantity = 6 };

        var updateresponse = await _authenticatedClient.PostAsJsonAsync(
            "/api/shoppingcart/items",
            itemupdate
        );

        Assert.Equal(HttpStatusCode.BadRequest, updateresponse.StatusCode);
    }

    [Fact]
    public async Task DeleteShoppingCart_ShouldReturn404_WhenCartDontExists()
    {
        var firstresponse = await _authenticatedClient.DeleteAsync("/api/shoppingcart");
        Assert.Equal(HttpStatusCode.NoContent, firstresponse.StatusCode);
        // the second response the cart will already be deleted so it should return
        var response = await _authenticatedClient.DeleteAsync("/api/shoppingcart");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
