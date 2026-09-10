# 🍕 APIzza

Proyecto de pizzeria con backend en **C# (ASP.NET Core Web API)** + Entity Framework Core + SQLite, documentado con **Swagger** (Swashbuckle), y una landing en HTML/CSS/JS puro que consume la API.

## Estructura

```
APIzza/
├── APIzza.sln
├── APIzza.Api/
│   ├── APIzza.Api.csproj
│   ├── appsettings.json
│   ├── Programs/
│   │   └── Program.cs               # Punto de entrada: arma EF Core, Swagger, CORS
│   ├── Biblioteca/
│   │   ├── ApizzaDbContext.cs       # DbContext de Entity Framework Core
│   │   └── SeedData.cs              # Crea la DB y carga pizzas de ejemplo
│   ├── Clases de las pizzas/
│   │   ├── Pizza.cs
│   │   ├── Cliente.cs
│   │   ├── Pedido.cs
│   │   └── ItemPedido.cs
│   ├── DTOs/                        # Los "shapes" que viajan en el body de los requests
│   │   ├── PizzaInputDto.cs
│   │   ├── ClienteInputDto.cs
│   │   ├── ItemPedidoDto.cs
│   │   ├── PedidoInputDto.cs
│   │   └── EstadoPedidoDto.cs
│   └── Controllers/
│       ├── PizzasController.cs
│       └── PedidosController.cs
└── landing/
    ├── index.html
    ├── css/style.css
    └── js/main.js
```

## 1. Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download) instalado (`dotnet --version` deberia mostrar 8.x).

## 2. Levantar el backend

```bash
cd APIzza.Api
dotnet restore
dotnet run
```

La primera vez que corre, va a:
- Crear el archivo SQLite `apizza.db` con las tablas (Pizzas, Clientes, Pedidos, ItemsPedido).
- Cargar 5 pizzas de ejemplo (solo si la tabla esta vacia).
- Levantar el server en `http://localhost:5000`.
- Publicar la documentacion Swagger en `http://localhost:5000/swagger`.

> Si preferis abrirlo con Visual Studio o Rider, abri `APIzza.sln` directamente.

## 3. Abrir la landing

Abri `landing/index.html` con Live Server (VS Code) o cualquier servidor estatico.
**No la abras con doble click (`file://`)**, porque el navegador bloquea el `fetch` a la API en ese modo.

La landing pide las pizzas a `http://localhost:5000/api/pizzas` y manda los pedidos a `http://localhost:5000/api/pedidos`.
Si tu Live Server no usa el puerto 5500, agrega su origen en la lista `WithOrigins(...)` de `Program.cs`.

## Endpoints principales

| Metodo | Ruta                     | Descripcion                       |
|--------|--------------------------|------------------------------------|
| GET    | /api/pizzas              | Lista pizzas (filtro ?categoria=)  |
| GET    | /api/pizzas/{id}         | Detalle de una pizza               |
| POST   | /api/pizzas              | Crear pizza                        |
| PUT    | /api/pizzas/{id}         | Editar pizza                       |
| DELETE | /api/pizzas/{id}         | Borrar pizza                       |
| POST   | /api/pedidos             | Crear pedido                       |
| GET    | /api/pedidos             | Listar pedidos                     |
| GET    | /api/pedidos/{id}        | Detalle de un pedido                |
| PATCH  | /api/pedidos/{id}/estado | Cambiar estado de un pedido        |

## Nota sobre este proyecto

Este codigo fue escrito a mano siguiendo la sintaxis estandar de ASP.NET Core 8 (minimal hosting + controllers) y Entity Framework Core 8, pero no se pudo compilar en el entorno donde se genero por no tener el SDK de .NET ni acceso a NuGet. Corre `dotnet restore` y `dotnet build` apenas lo bajes para confirmar que compile en tu maquina; si salta algun error de sintaxis avisame y lo corregimos.
