namespace Backend.Models;

// Review data returned by the API and accepted by the create endpoint.
public sealed class Review
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateOnly? ReviewDate { get; set; }
}