using ApiProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserLoginController : Controller
{
    private readonly Context _context;
    private readonly ILogger<UserLoginController> _logger;
    private readonly IPasswordHasher<Users> _passwordHasher;

    public UserLoginController(Context context, ILogger<UserLoginController> logger)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = new PasswordHasher<Users>();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) != PasswordVerificationResult.Success)
        {
            return Unauthorized(new { message = "Email o contraseña incorrectos." });
        }

        return Ok(new
        {
            message = "Inicio de sesión correcto.",
            user = new { user.Id, user.Name, user.Email }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest model)
    {
        if (await _context.Users.AnyAsync(user => user.Email == model.Email))
        {
            return Conflict(new { message = "Ya existe una cuenta con ese email." });
        }

        var user = new Users
        {
            Name = model.Name,
            Email = model.Email,
            Password = _passwordHasher.HashPassword(new Users(), model.Password),
            number = model.Number
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cuenta creada. Ya puedes iniciar sesión." });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(new { user.Id, user.Name, user.Email });
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest model)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        user.Name = model.Name;
        user.Email = model.Email;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuario actualizado correctamente." });
    }

    // Endpoint para eliminar una cuenta (Por lo general no se deberia usar, pero lo incluyo para fines de demostración)
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuario eliminado correctamente." });
    }

    // Usar dotnet add package Swashbuckle.AspNetCore.Annotations, para poder usar [SwaggerOperation] y documentar mejor los endpoints
    [HttpPost("logout")]
    [SwaggerOperation(Summary = "Cierra la sesión del usuario", Description = "Este endpoint cierra la sesión del usuario actual.")]
    public async Task<IActionResult> Logout()
    {
        return Ok(new { message = "Sesión cerrada correctamente." });
    }
}
