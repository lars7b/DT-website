namespace Backend.Models;
public sealed class Order
{
    public long Id{get;set;}
    public long CustomerId{get;set;}
    public DateTime OrderDate{get;set;}
    public string Status{get;set;}
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}