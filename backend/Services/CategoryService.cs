using Backend.Models;
using Backend.Repositories;
using StackExchange.Redis;
using System.Text.Json;
namespace Backend.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly CategoryRepository _repository;
    private readonly IDatabase _redis;

    public CategoryService(CategoryRepository repository,IConnectionMultiplexer redis)
    {
        _repository = repository;
        _redis = redis.GetDatabase();
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        string cacheKey = $"categories:all";
        var cachedData = await _redis.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            try
            {
                return JsonSerializer.Deserialize<IReadOnlyList<Category>>(cachedData!);
            }
            catch
            {
                await _redis.KeyDeleteAsync(cacheKey);
            }
        }
        var categories = await _repository.GetCategoriesAsync(cancellationToken);
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(categories),
            TimeSpan.FromMinutes(15)
        );
        return categories;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCategoryByIdAsync(id, cancellationToken);
    }
}