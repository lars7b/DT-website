public sealed class OrderStatusHistoryDto
{
    public long OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}