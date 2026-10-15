using ApiProject.Models;
using ApiProject.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// === 1. CONFIGURACIÓN DE SERVICIOS ===

builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<LogService>();
builder.Services.AddControllers();

// Configuración correcta para Swagger UI
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
}
    
);

var app = builder.Build();

// === 2. PIPELINE DE PETICIONES HTTP ===

// Todo lo de desarrollo debe ir junto al principio del pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Genera el JSON de Swagger
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API V1");
        // Opcional: Si quieres que al entrar a la raíz (https://localhost:XXXX/) 
        // abra Swagger directamente, descomenta la siguiente línea:
        // c.RoutePrefix = string.Empty; 
    });
}

app.UseHttpsRedirection();

// Configuración de Archivos Estáticos (FrontEnd)
var frontEndPath = Path.Combine(app.Environment.ContentRootPath, "FrontEnd");
if (Directory.Exists(frontEndPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontEndPath)
    });
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope()) // Cambiado a CreateScope para evitar bloqueos en el arranque
{
    var db = scope.ServiceProvider.GetRequiredService<Context>();
    // db.Database.EnsureCreated(); // ⚠️ NOTA: Si usas Migrate(), EnsureCreated() puede causar conflictos. Recomiendo comentarla.
    db.Database.Migrate();
}

app.Run();
