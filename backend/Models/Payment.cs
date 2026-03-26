namespace Backend.Models;

public sealed class Payment
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Method { get; set; } = string.Empty;
    public long OrderId { get; set; }
}
