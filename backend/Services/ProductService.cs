using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ProductService : IProductService
{
    private readonly ProductRepository _repository;

    public ProductService(ProductRepository repository)
    {
        _repository = repository;
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

        return await _repository.GetProductByIdAsync(id, cancellationToken);
    }

    public async Task<Product?> CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(product.Name) || product.Price < 0)
        {
            return null;
        }

        if (product.CategoryId is <= 0 || product.SubcategoryId is <= 0)
        {
            return null;
        }

        product.Name = product.Name.Trim();
        product.Description = string.IsNullOrWhiteSpace(product.Description) ? null : product.Description.Trim();

        return await _repository.CreateProductAsync(product, cancellationToken);
    }
}