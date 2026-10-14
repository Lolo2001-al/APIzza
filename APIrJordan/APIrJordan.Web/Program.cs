using System.Globalization;
using APIrJordan.Web.Data;
using APIrJordan.Web.Services;
using Microsoft.EntityFrameworkCore;

// Cultura invariante para toda la aplicación: los formularios y las URLs usan punto decimal ("40.5")
// y los montos se muestran a la manera argentina a través de Helpers/Formato (es-AR).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// MVC: controladores + vistas Razor
builder.Services.AddControllersWithViews();

// Entity Framework Core conectado a MySQL local.
// Host: 127.0.0.1 (localhost) | Puerto: 3306 | BD: apirjordan | Usuario: root
var connectionString = builder.Configuration.GetConnectionString("ApirJordanDb")
    ?? throw new InvalidOperationException("No se encontró ConnectionStrings:ApirJordanDb en appsettings.json.");

builder.Services.AddDbContext<ApirJordanDbContext>(opciones =>
    opciones.UseMySQL(connectionString));

// Sesión en memoria para el carrito (solo guarda ids, talles y cantidades)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.Cookie.Name = ".APIrJordan.Sesion";
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
    opciones.IdleTimeout = TimeSpan.FromHours(4);
});

// Servicio del carrito: necesita acceder a la sesión del pedido actual
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CarritoService>();

var app = builder.Build();

// Verifica/crea las tablas si todavía no se ejecutó database/schema.sql y carga el catálogo de ejemplo si está vacío.
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ApirJordanDbContext>();
    SeedData.Inicializar(contexto);
}

// Errores: en desarrollo se ve la página detallada de .NET; en producción, /Home/Error.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// 404 y otros códigos sin cuerpo se muestran con la misma página de error (sin cambiar la URL)
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
