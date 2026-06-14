using Backend.DTOs;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
// Handles HTTP requests for products and delegates logic to the service layer.
public sealed class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET /api/products with optional query filters.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? subcategoryId,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetProductsAsync(search, categoryId, subcategoryId, cancellationToken);
        return Ok(products);
    }

    // GET /api/products/{id} for details of one product.
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductByIdAsync(id, cancellationToken);
        if (product is null)
        {
            // No product with this id exists.
            return NotFound();
        }

        return Ok(product);
    }

    // POST /api/products to add a new product.
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product, CancellationToken cancellationToken)
    {
        var createdProduct = await _productService.CreateProductAsync(product, cancellationToken);
        if (createdProduct is null)
        {
            return BadRequest("Product moet een geldige naam en prijs hebben.");
        }

        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
    }
}