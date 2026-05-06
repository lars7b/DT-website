using Backend.Models;

namespace Backend.Services;

public interface IFavoriteService
{
    Task<bool> CustomerExistsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> ProductExistsAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FavoriteProduct>> GetFavoritesByCustomerAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> AddFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteAsync(int customerId, int productId, CancellationToken cancellationToken = default);
}