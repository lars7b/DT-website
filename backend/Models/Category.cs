namespace Backend.Models;

// Model dat een categorie voorstelt zoals die via de API wordt teruggegeven.
public sealed class Category
{
    // Unieke sleutel van de categorie in de database.
    public int Id { get; set; }

    // Naam van de categorie (bijv. "Chairs" of "Beds").
    public string? Name { get; set; }

    // Optionele beschrijving met extra uitleg over de categorie.
    public string? Description { get; set; }
}