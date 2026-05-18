using Backend.Models;

namespace Backend.Services;

public interface ISubcategoryService
{
    Task<IReadOnlyList<Subcategory>> GetSubcategoriesAsync(
        int? categoryId,
        CancellationToken cancellationToken = default);

    Task<Subcategory?> GetSubcategoryByIdAsync(int id, CancellationToken cancellationToken = default);
}