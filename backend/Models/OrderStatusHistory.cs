namespace Backend.Models;

/// <summary>
/// this class is for history keeping
/// specifically changes in order status
/// it is many to one to orders
/// </summary>
public sealed class OrderStatusHistory
{
    public long Id {get;set;}
    public long OrderId {get;set;}
    public string Status{get;set;}
    /// <summary>
    /// Date and time status got changed
    /// </summary>
    public DateTime Date {get;set;}
}