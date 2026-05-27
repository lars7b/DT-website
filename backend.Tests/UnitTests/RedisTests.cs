// [Fact]
// // cache hit 
// public async Task GetShoppingCartByUserIdAsync_ReturnsCachedCart_WhenCacheExists()
// {
//     // Arrange
//     long userId = 1;

//     var cachedCart = new ShoppingCartDto
//     {
//         Id = 10,
//         CustomerId = userId,
//         Items = new List<CartItemDto>()
//     };

//     string json = JsonSerializer.Serialize(cachedCart);

//     _redisMock
//         .Setup(r => r.StringGetAsync(
//             $"shopping_cart:{userId}",
//             It.IsAny<CommandFlags>()))
//         .ReturnsAsync(json);

//     // Act
//     var result = await _service.GetShoppingCartByUserIdAsync(userId);

//     // Assert
//     Assert.NotNull(result);
//     Assert.Equal(userId, result.CustomerId);

//     _repositoryMock.Verify(
//         r => r.GetAllItemsFromCartByCustomerId(
//             It.IsAny<long>(),
//             It.IsAny<CancellationToken>()),
//         Times.Never);
// }

// [Fact]
// // cache miss
// public async Task GetShoppingCartByUserIdAsync_LoadsFromRepository_WhenCacheMisses()
// {
//     // Arrange
//     long userId = 1;

//     _redisMock
//         .Setup(r => r.StringGetAsync(
//             $"shopping_cart:{userId}",
//             It.IsAny<CommandFlags>()))
//         .ReturnsAsync(RedisValue.Null);

//     var cartItems = new List<CartItem>
//     {
//         new CartItem
//         {
//             Id = 1,
//             CartId = 5,
//             ProductId = 100,
//             Quantity = 2
//         }
//     };

//     _repositoryMock
//         .Setup(r => r.GetAllItemsFromCartByCustomerId(
//             userId,
//             It.IsAny<CancellationToken>()))
//         .ReturnsAsync(cartItems);

//     // Act
//     var result = await _service.GetShoppingCartByUserIdAsync(userId);

//     // Assert
//     Assert.NotNull(result);

//     _repositoryMock.Verify(
//         r => r.GetAllItemsFromCartByCustomerId(
//             userId,
//             It.IsAny<CancellationToken>()),
//         Times.Once);

//     _redisMock.Verify(
//         r => r.StringSetAsync(
//             $"shopping_cart:{userId}",
//             It.IsAny<RedisValue>(),
//             TimeSpan.FromMinutes(15),
//             It.IsAny<bool>(),
//             It.IsAny<When>(),
//             It.IsAny<CommandFlags>()),
//         Times.Once);
// }

// [Fact]
// // cart empty
// public async Task GetShoppingCartByUserIdAsync_ReturnsNull_WhenRepositoryEmpty()
// {
//     // Arrange
//     long userId = 1;

//     _redisMock
//         .Setup(r => r.StringGetAsync(
//             It.IsAny<RedisKey>(),
//             It.IsAny<CommandFlags>()))
//         .ReturnsAsync(RedisValue.Null);

//     _repositoryMock
//         .Setup(r => r.GetAllItemsFromCartByCustomerId(
//             userId,
//             It.IsAny<CancellationToken>()))
//         .ReturnsAsync(new List<CartItem>());

//     // Act
//     var result = await _service.GetShoppingCartByUserIdAsync(userId);

//     // Assert
//     Assert.Null(result);

//     _redisMock.Verify(
//         r => r.StringSetAsync(
//             It.IsAny<RedisKey>(),
//             It.IsAny<RedisValue>(),
//             It.IsAny<TimeSpan>(),
//             It.IsAny<bool>(),
//             It.IsAny<When>(),
//             It.IsAny<CommandFlags>()),
//         Times.Never);
// }