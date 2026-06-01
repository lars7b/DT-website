// using System.Text;
using System.Text.Json;
using Backend.DTOs;
using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Moq;
using StackExchange.Redis;
using Xunit;

public class ShoppingCartServiceTests
{
    private readonly Mock<IShoppingCartRepository> _repoMock;
    private readonly Mock<IConnectionMultiplexer> _redisMock;
    private readonly Mock<IDatabase> _dbMock;

    private readonly ShoppingCartService _service;

    public ShoppingCartServiceTests()
    {
        _repoMock = new Mock<IShoppingCartRepository>();

        _redisMock = new Mock<IConnectionMultiplexer>();

        _dbMock = new Mock<IDatabase>();

        _redisMock
            .Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_dbMock.Object);

        _service = new ShoppingCartService(_repoMock.Object, _redisMock.Object);
    }

    [Fact]
    // cache hit
    public async Task GetShoppingCartByUserIdAsync_ReturnsCachedCart_WhenCacheExists()
    {
        // Arrange
        long userId = 1;

        var cachedCart = new ShoppingCartDto
        {
            Id = 10,
            CustomerId = userId,
            Items = new List<CartItemDto>(),
        };

        string json = JsonSerializer.Serialize(cachedCart);

        _dbMock
            .Setup(r => r.StringGetAsync($"shopping_cart:{userId}", It.IsAny<CommandFlags>()))
            .ReturnsAsync(json);

        // Act
        var result = await _service.GetShoppingCartByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.CustomerId);

        _repoMock.Verify(
            r => r.GetAllItemsFromCartByCustomerId(It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    // cache miss
    public async Task GetShoppingCartByUserIdAsync_LoadsFromRepository_WhenCacheMisses()
    {
        // Arrange
        long userId = 1;

        _dbMock
            .Setup(r => r.StringGetAsync($"shopping_cart:{userId}", It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var cartItems = new List<CartItem>
        {
            new CartItem
            {
                Id = 1,
                CartId = 5,
                ProductId = 100,
                Quantity = 2,
            },
        };

        _repoMock
            .Setup(r => r.GetAllItemsFromCartByCustomerId(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cartItems);

        // Act
        var result = await _service.GetShoppingCartByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);

        _repoMock.Verify(
            r => r.GetAllItemsFromCartByCustomerId(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );

        _dbMock.Verify(
    r => r.StringSetAsync(
        It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
        It.IsAny<TimeSpan?>(),
        It.IsAny<bool>(),
        It.IsAny<When>(),
        It.IsAny<CommandFlags>()
    ),
    Times.Once
);
    }

    [Fact]
    // cart empty
    public async Task GetShoppingCartByUserIdAsync_ReturnsNull_WhenRepositoryEmpty()
    {
        // Arrange
        long userId = 1;

        _dbMock
            .Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        _repoMock
            .Setup(r => r.GetAllItemsFromCartByCustomerId(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem>());

        // Act
        var result = await _service.GetShoppingCartByUserIdAsync(userId);

        // Assert
        Assert.Null(result);

        _dbMock.Verify(
    r => r.StringSetAsync(
        It.IsAny<RedisKey>(),
        It.IsAny<RedisValue>(),
        It.IsAny<TimeSpan?>(),
        It.IsAny<bool>(),
        It.IsAny<When>(),
        It.IsAny<CommandFlags>()
    ),
    Times.Once
);
    }
}
