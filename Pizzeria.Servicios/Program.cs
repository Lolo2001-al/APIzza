using Pizzeria.Servicios.Clases;

// Uso:  dotnet run --project Pizzeria.Servicios -- cocina
//       dotnet run --project Pizzeria.Servicios -- reparto
var rol = args.Length > 0 ? args[0] : "cocina";
await new ServicioInterno(rol).IniciarAsync();
