using Backend.Models;

namespace Backend.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        CancellationToken cancellationToken = default);

    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
}