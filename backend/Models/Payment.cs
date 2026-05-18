namespace Backend.Models;

public sealed class Payment
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    /// <summary>
    /// Status can be "Pending", "Completed" or "Paid", "Failed", etc.
    /// </summary>
    public string Status { get; set; } = "Pending";
    public long OrderId { get; set; }
}
