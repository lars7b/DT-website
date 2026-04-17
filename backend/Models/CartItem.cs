namespace Backend.Models;

public class CartItem : IModel
{
    public long Id { get; set; }
    public long CartId { get; set; }
    public long ProductId { get; set; }
    public int Quantity { get; set; }
}
