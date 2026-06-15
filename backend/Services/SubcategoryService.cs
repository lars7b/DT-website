using System.Text.Json;
using Backend.Models;
using Backend.Repositories;
using StackExchange.Redis;

namespace Backend.Services;

public sealed class SubcategoryService : ISubcategoryService
{
    private readonly SubcategoryRepository _repository;
    private readonly IDatabase _redis;

    public SubcategoryService(SubcategoryRepository repository, IConnectionMultiplexer redis)
    {
        _repository = repository;
        _redis = redis.GetDatabase();
    }

    public async Task<IReadOnlyList<Subcategory>> GetSubcategoriesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default
    )
    {
        string cacheKey = "sub_categories:";
        if (categoryId == null)
        {
            cacheKey += "all";
        }
        else
        {
            cacheKey += $"cetagory:{categoryId}";
        }
        var cachedData = await _redis.StringGetAsync(cacheKey);
        if (!cachedData.IsNullOrEmpty)
        {
            try
            {
                return JsonSerializer.Deserialize<IReadOnlyList<Subcategory>>(cachedData!);
            }
            catch
            {
                await _redis.KeyDeleteAsync(cacheKey);
            }
        }
        var categories = await _repository.GetSubcategoriesAsync(categoryId, cancellationToken);
        await _redis.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(categories),
            TimeSpan.FromMinutes(15)
        );
        return categories;
    }

    public async Task<Subcategory?> GetSubcategoryByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _repository.GetSubcategoryByIdAsync(id, cancellationToken);
    }
}
