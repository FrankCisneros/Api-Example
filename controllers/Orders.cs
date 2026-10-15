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


}
