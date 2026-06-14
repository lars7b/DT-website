namespace Backend.DTOs;

public sealed class OrderItemDto
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}