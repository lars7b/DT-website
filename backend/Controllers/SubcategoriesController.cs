using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class SubcategoriesController : ControllerBase
{
    private readonly ISubcategoryService _subcategoryService;

    public SubcategoriesController(ISubcategoryService subcategoryService)
    {
        _subcategoryService = subcategoryService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Subcategory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Subcategory>>> GetSubcategories(
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken)
    {
        var subcategories = await _subcategoryService.GetSubcategoriesAsync(categoryId, cancellationToken);
        return Ok(subcategories);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Subcategory), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Subcategory>> GetSubcategoryById(int id, CancellationToken cancellationToken)
    {
        var subcategory = await _subcategoryService.GetSubcategoryByIdAsync(id, cancellationToken);
        if (subcategory is null)
        {
            return NotFound();
        }

        return Ok(subcategory);
    }
}