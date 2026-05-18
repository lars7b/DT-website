namespace Backend.Models;

public sealed class Subcategory
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}