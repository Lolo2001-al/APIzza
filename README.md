# 🍕 APIzza

Pizzería hecha con **ASP.NET Core MVC (C#)**, **Entity Framework Core 10** y **MySQL 8+**.

La aplicación guarda en MySQL los clientes, pedidos, líneas de pedido y pizzas del menú.

## Estructura

```text
APIzza/
├── APIzza.sln
├── database/
│   ├── schema.sql                # crea la BD y las tablas
│   ├── inserts.sql               # datos iniciales y pedido de prueba
│   └── DER/                      # DER en PNG/SVG/Mermaid
└── APIzza.Web/
    ├── Program.cs                # conexión MySQL y arranque MVC
    ├── Models/                   # Pizza, Cliente, Pedido, ItemPedido
    ├── ViewModels/               # datos de formularios/vistas
    ├── Data/                     # ApizzaDbContext + SeedData
    ├── Controllers/              # Home, Pizzas, Pedidos
    ├── Views/                    # vistas Razor
    └── wwwroot/                  # CSS e imágenes
```

## Preparar MySQL

Requiere **MySQL Server 8.x o superior** y el servicio de MySQL iniciado. No hace falta XAMPP ni Docker.

1. Abrí MySQL Workbench o la consola de MySQL.
2. Ejecutá `database/schema.sql`. Este script crea `apizza` y sus tablas.
3. Ejecutá `database/inserts.sql`. Carga las cinco pizzas, un cliente y un pedido de prueba.

> `schema.sql` elimina y recrea las tablas. No lo vuelvas a ejecutar si ya tenés datos que quieras conservar.

## Conexión MySQL → página web

Archivo: `APIzza.Web/appsettings.json`

```text
Server=127.0.0.1;Port=3306;Database=apizza;User=root;Password=CAMBIAR_ESTO;SslMode=None;
```

Significado de cada parte:

- `Server=127.0.0.1`: MySQL está en tu propia computadora. Es equivalente a `localhost`.
- `Port=3306`: puerto habitual del servidor MySQL.
- `Database=apizza`: base que crean los scripts SQL.
- `User=root`: usuario de MySQL.
- `Password=CAMBIAR_ESTO`: reemplazalo por la contraseña que elegiste al instalar MySQL.

La URL que ves al ejecutar la web, por ejemplo `http://localhost:5000`, corresponde a **ASP.NET**. La conexión a la base usa el `127.0.0.1:3306` indicado arriba.

## Ejecutar la página

Requiere [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

Desde PowerShell, entrando a la carpeta que contiene `APIzza.Web.csproj`:

```powershell
cd .\APIzza.Web
dotnet restore
dotnet build
dotnet run
```

Usá la URL que muestre `dotnet run`.

## Qué se guarda en MySQL

| Acción en la web | Tabla |
|---|---|
| Confirmar pedido y datos del cliente | `Clientes` |
| Crear el pedido | `Pedidos` |
| Pizza + cantidad + precio del pedido | `ItemsPedido` |
| Agregar una nueva pizza | `Pizzas` |

## Agregar una nueva pizza

Desde **Menú → Agregar pizza** se puede crear una pizza nueva indicando nombre, descripción, precio y categoría. La categoría determina la sección del menú en la que aparece.

## Páginas

| URL | Qué hace |
|---|---|
| `/` | Inicio |
| `/Pizzas` | Menú y filtros por categoría |
| `/Pizzas/Crear` | Alta de una nueva pizza |
| `/Pizzas/Detalles/1` | Detalle de pizza |
| `/Pedidos/Crear` | Formulario de pedido + datos del cliente |
| `/Pedidos` | Lista de pedidos guardados |
| `/Pedidos/Detalles/1` | Detalle de un pedido |

Ver `README_MYSQL.md` para la guía específica de conexión.
