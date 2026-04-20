namespace Backend.Models;

public sealed class Payment{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status{get;set;}
    public long OrderId { get; set; }
}