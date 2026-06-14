namespace Backend.Models;

public sealed class Order
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Status can be "Pending", "Processing", "Shipped", "Delivered", "Cancelled", etc.
    /// </summary>
    public string Status { get; set; } = "Pending";
    public string? TrackingNumber { get; set; }

    public string? Carrier { get; set; }
    public List<OrderStatusHistory>? History {get;set;}
    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
}
