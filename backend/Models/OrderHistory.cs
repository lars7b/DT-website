namespace Backend.Models;

public sealed class OrderHistory
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
    public List<OrderHistoryItem> Items { get; set; } = new();
}

public sealed class OrderHistoryItem
{
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
