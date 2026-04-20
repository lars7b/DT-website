namespace Backend.DTOs;

public sealed class ShoppingCartDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
}