using Backend.Models;

namespace Backend.Services;

public interface IReviewService
{
    Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    Task<Review?> CreateReviewAsync(Review review, CancellationToken cancellationToken = default);
}