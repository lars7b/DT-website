namespace Backend.DTOs;

public sealed class CreatePaymentDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Status { get; set; } = "Pending";
    /// <summary>
    /// it could be null in that case check orders that are not yet paid/pending and use that order id
    /// </summary>
    public long? OrderId { get; set; }
}
