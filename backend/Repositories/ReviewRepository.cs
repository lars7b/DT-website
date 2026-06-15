using Backend.Models;
using Npgsql;

namespace Backend.Repositories;

public class ReviewRepository
{
    private readonly string _connectionString;

    public ReviewRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    public async Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT id,
                                    customer_id,
                                    product_id,
                                    rating,
                                    comment,
                                    review_date
                             FROM reviews
                             WHERE product_id = @productId
                             ORDER BY review_date DESC, id DESC";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("productId", productId);

        var reviews = new List<Review>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            reviews.Add(MapReview(reader));
        }

        return reviews;
    }

    public async Task<Review> CreateReviewAsync(Review review, CancellationToken cancellationToken = default)
    {
        const string sql = @"INSERT INTO reviews (customer_id, product_id, rating, comment, review_date)
                             VALUES (@customerId, @productId, @rating, @comment, @reviewDate)
                             RETURNING id,
                                       customer_id,
                                       product_id,
                                       rating,
                                       comment,
                                       review_date";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("customerId", review.CustomerId);
        command.Parameters.AddWithValue("productId", review.ProductId);
        command.Parameters.AddWithValue("rating", review.Rating);
        command.Parameters.AddWithValue("comment", (object?)review.Comment ?? DBNull.Value);
        command.Parameters.AddWithValue("reviewDate", review.ReviewDate ?? DateOnly.FromDateTime(DateTime.UtcNow));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return MapReview(reader);
    }

    private static Review MapReview(NpgsqlDataReader reader)
    {
        return new Review
        {
            Id = reader.GetInt32(reader.GetOrdinal("id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
            ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
            Rating = reader.GetInt32(reader.GetOrdinal("rating")),
            Comment = reader.IsDBNull(reader.GetOrdinal("comment"))
                ? null
                : reader.GetString(reader.GetOrdinal("comment")),
            ReviewDate = reader.IsDBNull(reader.GetOrdinal("review_date"))
                ? null
                : reader.GetFieldValue<DateOnly>(reader.GetOrdinal("review_date"))
        };
    }
}