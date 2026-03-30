namespace Backend.Models;

public sealed class Payment :IModel
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public long OrderId { get; set; }
}