using APIzza.Api.Biblioteca;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

const string PoliticaCorsLanding = "PoliticaCorsLanding";

// Fija un puerto fijo y conocido para que la landing siempre sepa donde pegarle.
builder.WebHost.UseUrls("http://localhost:5000");

// --- Servicios ---

builder.Services.AddControllers();

builder.Services.AddDbContext<ApizzaDbContext>(opciones =>
    opciones.UseSqlite(
        builder.Configuration.GetConnectionString("ApizzaDb") ?? "Data Source=apizza.db"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "APIzza - API de la Pizzeria",
        Version = "v1",
        Description = "API REST en C# / ASP.NET Core para gestionar el menu de pizzas y los pedidos de la pizzeria APIzza.",
    });

    // Incorpora los comentarios /// <summary> de los controllers a la doc de Swagger
    var archivoXml = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var rutaXml = Path.Combine(AppContext.BaseDirectory, archivoXml);
    if (File.Exists(rutaXml))
    {
        opciones.IncludeXmlComments(rutaXml);
    }
});

// La landing corre en otro origen (ej. Live Server), asi que necesita permiso CORS explicito.
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy(PoliticaCorsLanding, politica =>
    {
        politica
            .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Crea la base de datos (si no existe) y la siembra con pizzas de ejemplo
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ApizzaDbContext>();
    SeedData.Inicializar(contexto);
}

// --- Middlewares ---

app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "APIzza API v1");
});

app.UseCors(PoliticaCorsLanding);

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    mensaje = "Bienvenido a la API de APIzza",
    documentacion = "/swagger",
}));

app.Run();
