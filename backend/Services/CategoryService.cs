using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly CategoryRepository _repository;

    public CategoryService(CategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetCategoriesAsync(cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetCategoryByIdAsync(id, cancellationToken);
    }
}