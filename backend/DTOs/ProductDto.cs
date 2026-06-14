namespace Backend.DTOs;

public sealed class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? CategoryName { get; set; }
    public string? SubcategoryName { get; set; }
}