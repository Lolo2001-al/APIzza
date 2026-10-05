using APIzza.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC: registra controladores + vistas Razor
builder.Services.AddControllersWithViews();

// Entity Framework Core con SQLite (el "Model" se guarda aca)
builder.Services.AddDbContext<ApizzaDbContext>(opciones =>
    opciones.UseSqlite(
        builder.Configuration.GetConnectionString("ApizzaDb") ?? "Data Source=apizza.db"));

var app = builder.Build();

// Crea la base (si no existe) y carga las pizzas de ejemplo
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ApizzaDbContext>();
    SeedData.Inicializar(contexto);
}

app.UseStaticFiles();   // sirve wwwroot (css)
app.UseRouting();

// Ruta por defecto de MVC: /Controlador/Accion/Id
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
