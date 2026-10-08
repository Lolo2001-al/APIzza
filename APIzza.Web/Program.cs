using APIzza.Web.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// MVC: registra controladores + vistas Razor
builder.Services.AddControllersWithViews();

// Entity Framework Core conectado a MySQL local.
// Host: 127.0.0.1 (localhost) | Puerto: 3306 | BD: apizza | Usuario: root
var connectionString = builder.Configuration.GetConnectionString("ApizzaDb")
    ?? throw new InvalidOperationException("No se encontró ConnectionStrings:ApizzaDb en appsettings.json.");

builder.Services.AddDbContext<ApizzaDbContext>(opciones =>
    opciones.UseMySQL(connectionString));

var app = builder.Build();

// Verifica/crea las tablas si el usuario todavía no ejecutó schema.sql.
// Si ya ejecutó schema.sql, EF detecta la base existente y no vuelve a cargar el menú.
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ApizzaDbContext>();
    SeedData.Inicializar(contexto);
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
