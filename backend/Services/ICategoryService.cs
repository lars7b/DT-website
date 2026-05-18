using Backend.Models;

namespace Backend.Services;

// Interface met alle categorie-acties die de controller nodig heeft.
public interface ICategoryService
{
    // Haalt alle categorieen op, gesorteerd op id.
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    // Haalt precies een categorie op via id.
    // Geeft null terug als de categorie niet bestaat.
    Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
}