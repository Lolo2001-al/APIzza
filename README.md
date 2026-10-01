# Pizzería Don Vito

Tres terminales, desde esta carpeta (necesitás el SDK de .NET 8 o superior):

    dotnet run --project Pizzeria.Servicios -- cocina
    dotnet run --project Pizzeria.Servicios -- reparto
    dotnet run --project Pizzeria.Api

- Landing: http://localhost:5000
- Swagger: http://localhost:5000/swagger

Para probar un error: apagá la cocina (Ctrl+C) y hacé un pedido. La API reintenta 3 veces y lo cancela con un mensaje claro.

## Estructura

    Pizzeria.Api/
      Program.cs           arma la app, Swagger y conecta los controladores
      Controladores/        PizzasController, PedidosController (MVC, asincrónicos)
      Clases/               Model: Pizza, Cliente, ItemPedido, NuevoPedido, Linea, Pedido,
                             EstadoPedido, RepositorioPizzas, RepositorioPedidos, Despachador
      wwwroot/               landing (index.html), la Vista que consume la API
    Pizzeria.Servicios/
      Program.cs           arranque
      Clases/               ServicioInterno (cocina y reparto)
