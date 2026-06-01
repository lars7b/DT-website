using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public sealed class SubcategoryService : ISubcategoryService
{
    private readonly SubcategoryRepository _repository;

    public SubcategoryService(SubcategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Subcategory>> GetSubcategoriesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetSubcategoriesAsync(categoryId, cancellationToken);
    }

    public async Task<Subcategory?> GetSubcategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetSubcategoryByIdAsync(id, cancellationToken);
    }
}