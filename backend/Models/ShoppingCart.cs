namespace Backend.Models;

public class ShoppingCart
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
}
