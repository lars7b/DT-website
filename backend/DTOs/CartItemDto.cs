namespace Backend.DTOs;

public sealed class CartItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public decimal? PricePerUnit { get; set; }
    public int Quantity { get; set; }
}
