using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests;

public sealed class EndpointsSmokeTests
{
    [Fact]
    public async Task Categories_GetCategories_ReturnsOk()
    {
        // Arrange: we maken een nep service die alvast data teruggeeft.
        // Zo kunnen we controleren dat de controller 200 OK terugstuurt met dezelfde data.
        var service = new Mock<ICategoryService>();
        var categories = new List<Category> { new Category { Id = 1, Name = "Snacks" } };
        service.Setup(s => s.GetCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var controller = new CategoriesController(service.Object);

        // Act: we roepen de endpoint methode aan zoals een echte HTTP GET zou doen.
        var result = await controller.GetCategories(CancellationToken.None);

        // Assert: we verwachten een OkObjectResult (HTTP 200) met dezelfde lijst.
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<Category>>(ok.Value);
        Assert.Single(payload);
    }

    [Fact]
    public async Task Categories_GetCategoryById_ReturnsNotFound_WhenMissing()
    {
        // Arrange: de service geeft null terug, dus de controller moet 404 teruggeven.
        var service = new Mock<ICategoryService>();
        service.Setup(s => s.GetCategoryByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var controller = new CategoriesController(service.Object);

        // Act: we roepen de endpoint methode aan voor een id die niet bestaat.
        var result = await controller.GetCategoryById(99, CancellationToken.None);

        // Assert: 404 Not Found omdat er geen categorie is.
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Subcategories_GetSubcategories_ReturnsOk()
    {
        // Arrange: we geven een lijst terug zodat de controller 200 OK kan sturen.
        var service = new Mock<ISubcategoryService>();
        var subcategories = new List<Subcategory> { new Subcategory { Id = 3, Name = "Chips" } };
        service.Setup(s => s.GetSubcategoriesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subcategories);

        var controller = new SubcategoriesController(service.Object);

        // Act: we roepen de endpoint methode aan met een categoryId filter.
        var result = await controller.GetSubcategories(1, CancellationToken.None);

        // Assert: 200 OK met dezelfde subcategorieen lijst.
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<Subcategory>>(ok.Value);
        Assert.Single(payload);
    }

    [Fact]
    public async Task Subcategories_GetSubcategoryById_ReturnsNotFound_WhenMissing()
    {
        // Arrange: service geeft null terug, dus de controller moet 404 teruggeven.
        var service = new Mock<ISubcategoryService>();
        service.Setup(s => s.GetSubcategoryByIdAsync(55, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subcategory?)null);

        var controller = new SubcategoriesController(service.Object);

        // Act: we roepen de endpoint methode aan voor een niet-bestaande id.
        var result = await controller.GetSubcategoryById(55, CancellationToken.None);

        // Assert: 404 Not Found omdat de subcategorie ontbreekt.
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Favorites_GetFavorites_ReturnsNotFound_WhenCustomerMissing()
    {
        // Arrange: klant bestaat niet, dus GET moet 404 teruggeven.
        var service = new Mock<IFavoriteService>();
        service.Setup(s => s.CustomerExistsAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new FavoritesController(service.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(
                        new[]
                        {
                            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "42"),
                        },
                        "TestAuth"
                    )
                ),
            },
        };

        // Act: we roepen de endpoint methode aan voor favorites van klant 42.
        var result = await controller.GetFavorites(CancellationToken.None);
        // Assert: 404 omdat de klant niet bestaat.
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Favorites_AddFavorite_ReturnsCreated_WhenAdded()
    {
        // Arrange: klant en product bestaan en toevoegen lukt.
        var service = new Mock<IFavoriteService>();
        service.Setup(s => s.CustomerExistsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        service.Setup(s => s.ProductExistsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        service.Setup(s => s.AddFavoriteAsync(7, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

var controller = new FavoritesController(service.Object);
controller.ControllerContext = new ControllerContext
{
    HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
    {
        User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "7"),
                },
                "TestAuth"
            )
        ),
    },
};

// Act: we roepen de endpoint methode aan die een favorite toevoegt.
var result = await controller.AddFavorite(10, CancellationToken.None);
        // Assert: 201 Created en verwijzing naar de GetFavorites route.
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(FavoritesController.GetFavorites), created.ActionName);
    }

    [Fact]
    public async Task Favorites_RemoveFavorite_ReturnsNoContent_WhenRemoved()
    {
        // Arrange: klant bestaat en verwijderen lukt.
        var service = new Mock<IFavoriteService>();
        service.Setup(s => s.CustomerExistsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        service.Setup(s => s.RemoveFavoriteAsync(7, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = new FavoritesController(service.Object);

        // Act: we roepen de endpoint methode aan die een favorite verwijdert.
        var result = await controller.RemoveFavorite(7, 10, CancellationToken.None);

        // Assert: 204 No Content betekent dat de delete is uitgevoerd.
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Orders_GetOrderHistory_ReturnsNotFound_WhenCustomerMissing()
    {
        // Arrange: klant bestaat niet, dus de endpoint moet 404 geven.
        var service = new Mock<IOrderHistoryService>();
        service.Setup(s => s.CustomerExistsAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = new OrdersController(service.Object);

        // Act: we roepen de endpoint methode aan voor de order history.
        var result = await controller.GetOrderHistory(12, CancellationToken.None);

        // Assert: 404 omdat de klant niet bestaat.
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
