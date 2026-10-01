using System.Text.Json.Serialization;
using Pizzeria.Api.Clases;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Los repositorios son el Model: guardan los datos y los controladores los piden por inyección de dependencias.
builder.Services.AddSingleton<RepositorioPizzas>();
builder.Services.AddSingleton<RepositorioPedidos>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Pizzería Don Vito API",
        Version = "v1",
        Description = "Recibe pedidos de pizza y los delega a la cocina y al reparto por sockets."
    });
    var xml = Path.Combine(AppContext.BaseDirectory, "Pizzeria.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();
app.UseSwagger();       // JSON en /swagger/v1/swagger.json
app.UseSwaggerUI();     // documentación en /swagger
app.UseDefaultFiles();  // sirve wwwroot/index.html en "/"
app.UseStaticFiles();

app.MapControllers();   // conecta las rutas [Route]/[HttpGet]/[HttpPost] de Controladores/

app.Run();
