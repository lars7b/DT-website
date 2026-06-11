using Backend.Models;
using Backend.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace Backend.Services;

public class ProductService : IProductService
{
    private readonly ProductRepository _repository;
    private readonly IDatabase _redis;

    public ProductService(ProductRepository repository,IConnectionMultiplexer redis)
    {
        _repository = repository;
        _redis = redis.GetDatabase();
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        CancellationToken cancellationToken = default)
    {
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        return await _repository.GetProductsAsync(search, categoryId, subcategoryId, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return null;
        }
        string cacheKey = $"product:{id}";
        var cachedData = await _redis.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            try
            {
                return JsonSerializer.Deserialize<Product>(cachedData!);
            }
            catch
            {
                await _redis.KeyDeleteAsync(cacheKey);
            }
        }
        Product? product = await _repository.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return null;
        }
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(product),
            TimeSpan.FromMinutes(15)
        );
        return product;
    }
}