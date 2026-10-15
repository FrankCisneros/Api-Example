using ApiProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using ApiProject.Services;


namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : Controller
{
    private readonly Context _context;
    private readonly LogService _logService;

    public OrdersController(Context context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }

    [HttpGet("Orders")]
    [SwaggerOperation(Summary = "Obtiene todos los pedidos", Description = "Este endpoint devuelve una lista de todos los pedidos realizados.")]
    public async Task<IActionResult> GetAllOrders(int page = 1, int size = 10)
    {
        if (page < 1) page = 1;
        if (size < 1 || size > 100) size = 10;

        int skip = (page - 1) * size;
        var orders = await _context.Orders.Skip(skip).Take(size).ToListAsync();

        var data = await _context.Orders
            .AsNoTracking().OrderBy(o => o.Id).Skip(skip).Take(size).ToListAsync();

        return Ok(new
        {
            page,
            size,
            total = await _context.Orders.CountAsync(),
            Data = data
        });
    }

    [HttpGet("Orders/{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromBody] List<OrderRequest> orderRequest)
    {
        if (orderRequest == null || !orderRequest.Any())
        {
            return BadRequest("No se proporcionaron pedidos para crear.");
        }

        var orders = orderRequest.Select(o => new Orders
        {
            ProductId = o.ProductId,
            Quantity = o.Quantity,
            OrderDate = DateTime.UtcNow
        }).ToList();

        await _context.Orders.AddRangeAsync(orders);
        await _context.SaveChangesAsync();

        // Log the creation of orders

        await _logService.Log(Services.LogLevel.Info, $"{orders.Count} pedidos creados exitosamente.");

        return Ok(new { message = $"{orders.Count} pedidos creados exitosamente." });
    }

    
}
