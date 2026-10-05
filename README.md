# 🍕 APIzza

Pizzería hecha con **ASP.NET Core MVC** (C#), **Entity Framework Core** y **SQLite**.

## Estructura

```
APIzza/
├── APIzza.sln
└── APIzza.Web/
    ├── Program.cs                  # Arranque: servicios, base de datos, ruta MVC
    ├── Models/                     # M: Pizza, Cliente, Pedido, ItemPedido
    ├── ViewModels/                 # Objetos armados para una vista (MenuViewModel, PedidoViewModel)
    ├── Data/                       # ApizzaDbContext (EF Core) + SeedData (pizzas de ejemplo)
    ├── Controllers/                # C: Home, Pizzas, Pedidos (todos async)
    ├── Views/                      # V: Razor (.cshtml)
    │   ├── Shared/_Layout.cshtml   # Estructura común (barra, pie)
    │   ├── Shared/_TarjetaPizza.cshtml
    │   ├── Home/ Pizzas/ Pedidos/
    ├── Helpers/PizzaImagen.cs       # Elige la ilustración de cada pizza
    └── wwwroot/                    # Estilos e ilustraciones locales
```

## Cómo correrlo

Requiere [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
cd APIzza.Web
dotnet restore
dotnet build
dotnet run
```

Abrir `http://localhost:5000`. La primera vez crea `apizza.db` con 5 pizzas de ejemplo.

## Páginas

| URL                    | Controlador / Acción        | Qué hace                                   |
|------------------------|-----------------------------|--------------------------------------------|
| `/`                    | Home / Index                | Landing con categorías, pizzas destacadas y acceso a pedidos |
| `/Pizzas`              | Pizzas / Index              | Menú, con filtro `?categoria=`             |
| `/Pizzas/Detalles/1`   | Pizzas / Detalles           | Detalle de una pizza                       |
| `/Pedidos/Crear`       | Pedidos / Crear (GET y POST)| Formulario para hacer un pedido            |
| `/Pedidos`             | Pedidos / Index             | Lista de pedidos                           |
| `/Pedidos/Detalles/1`  | Pedidos / Detalles          | Detalle de un pedido                       |

Ver `GUIA_DEFENSA.md` para la explicación de MVC, el código asincrónico y cómo el controlador le pasa el Model a la vista.
