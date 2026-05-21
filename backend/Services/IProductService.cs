using Backend.Models;

namespace Backend.Services;

// Defines product-related business operations used by controllers.
public interface IProductService
{
    // Returns all products, optionally filtered by search/category/subcategory.
    Task<IReadOnlyList<Product>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? subcategoryId,
        CancellationToken cancellationToken = default);

    // Returns one product by id, or null when not found.
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    // Creates a new product and returns the stored record, or null when validation fails.
    Task<Product?> CreateProductAsync(Product product, CancellationToken cancellationToken = default);
}