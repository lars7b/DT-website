using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ReviewService : IReviewService
{
    private readonly ReviewRepository _repository;

    public ReviewService(ReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return Array.Empty<Review>();
        }

        return await _repository.GetReviewsByProductIdAsync(productId, cancellationToken);
    }

    public async Task<Review?> CreateReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        if (review.ProductId <= 0 || review.CustomerId <= 0 || review.Rating is < 1 or > 5)
        {
            return null;
        }

        review.Comment = string.IsNullOrWhiteSpace(review.Comment) ? null : review.Comment.Trim();
        review.ReviewDate ??= DateOnly.FromDateTime(DateTime.UtcNow);

        return await _repository.CreateReviewAsync(review, cancellationToken);
    }
}