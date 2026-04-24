namespace Backend.DTOs;

public sealed class OrderDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    /// <summary>
    /// Status can be "Pending", "Processing", "Shipped", "Delivered", "Cancelled", etc.
    /// </summary>
    public string Status { get; set; } = "Pending";
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}