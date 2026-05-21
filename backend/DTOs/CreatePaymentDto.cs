namespace Backend.DTOs;

public sealed class CreatePaymentDto
{
    public long Id { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    }