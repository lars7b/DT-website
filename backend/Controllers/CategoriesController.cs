using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
// Verwerkt HTTP-requests voor categorieen en delegeert de logica naar de service-laag.
public sealed class CategoriesController : ControllerBase
{
    // Service met alle categorie-gerelateerde operaties.
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET /api/categories
    // Geeft een lijst met alle categorieen terug.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Category>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Category>>> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetCategoriesAsync(cancellationToken);
        return Ok(categories);
    }

    // GET /api/categories/{id}
    // Geeft een specifieke categorie terug op basis van id.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Category>> GetCategoryById(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
        if (category is null)
        {
            // Als de categorie niet bestaat, sturen we 404 terug.
            return NotFound();
        }

        return Ok(category);
    }
}