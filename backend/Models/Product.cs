namespace Backend.Models;

// Product data returned by the API.
public sealed class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }

    // IDs from related category tables.
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }

    // Names from SQL joins, useful for frontend display.
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
}