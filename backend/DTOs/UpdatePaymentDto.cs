namespace Backend.DTOs;

public sealed class UpdatePaymentDto
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public long OrderId { get; set; }
    public string Status { get; set; } = null!;
}
