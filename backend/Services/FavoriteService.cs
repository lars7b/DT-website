using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public sealed class FavoriteService : IFavoriteService
{
    private readonly FavoriteRepository _repository;

    public FavoriteService(FavoriteRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken = default)
    {
        return await _repository.CustomerExistsAsync(customerId, cancellationToken);
    }

    public async Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _repository.ProductExistsAsync(productId, cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteProduct>> GetFavoritesByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetFavoritesByCustomerAsync(customerId, cancellationToken);
    }

    public async Task<bool> AddFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        return await _repository.AddFavoriteAsync(customerId, productId, cancellationToken);
    }

    public async Task<bool> RemoveFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default)
    {
        return await _repository.RemoveFavoriteAsync(customerId, productId, cancellationToken);
    }
}