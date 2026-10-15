using ApiProject.Models;
using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : Controller
{
    private readonly Context _context;
    private readonly ILogger<ProductController> _logger;

    public ProductController(Context context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Obtiene todos los productos", Description = "Este endpoint devuelve una lista de todos los productos disponibles.")]
    public async Task<IActionResult> GetAllProducts(int page = 1, int size = 10)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 10;

        int skip = (page - 1) * size;
        var products = await _context.Productos.Skip(skip).Take(size).ToListAsync();

        var data = await _context.Productos
        .AsNoTracking().OrderBy(p => p.Id).Skip(skip).Take(size).ToListAsync();

        return Ok(new
        {
            page,
            size,
            total = await _context.Productos.CountAsync(),
            Data = data
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] List<ProductRequest> productRequest)
    {
        if (productRequest == null || !productRequest.Any())
        {
            return BadRequest("No se proporcionaron productos para crear.");
        }

        var products = productRequest.Select(req => new Productos
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            CategoryId = req.CategoryId
        }).ToList();

        _context.Productos.AddRange(products);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, products);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequest productRequest)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        product.Name = productRequest.Name;
        product.Description = productRequest.Description;
        product.Price = productRequest.Price;
        product.CategoryId = productRequest.CategoryId;

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Productos.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }

    [HttpPost("Categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoriesRequest category)
    {
        var categoryModel = new Categories
        {
            Name = category.Name
        };

        _context.Categories.Add(categoryModel);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAllCategories), new { id = categoryModel.Id }, categoryModel);
    }
}
