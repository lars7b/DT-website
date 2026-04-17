namespace Backend.Models;

public class ShoppingCart : IModel
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
}
