namespace Backend.DTOs;

public sealed class CartItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
}
