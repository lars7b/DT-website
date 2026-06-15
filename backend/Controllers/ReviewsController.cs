using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
// Handles HTTP requests for reviews and delegates logic to the service layer.
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // GET /api/reviews/product/{productId} for all reviews of one product.
    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<Review>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Review>>> GetReviewsByProductId(int productId, CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId, cancellationToken);
        return Ok(reviews);
    }

    // POST /api/reviews to place a new review.
    [HttpPost]
    [ProducesResponseType(typeof(Review), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Review>> CreateReview([FromBody] Review review, CancellationToken cancellationToken)
    {
        var createdReview = await _reviewService.CreateReviewAsync(review, cancellationToken);
        if (createdReview is null)
        {
            return BadRequest("Review moet een geldig customerId, productId en rating tussen 1 en 5 hebben.");
        }

        return CreatedAtAction(
            nameof(GetReviewsByProductId),
            new { productId = createdReview.ProductId },
            createdReview);
    }
}