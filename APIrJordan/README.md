# 👟 APIr Jordan

Tienda de zapatillas Jordan hecha con **ASP.NET Core MVC (C#)**, **Entity Framework Core 10** y **MySQL 8+**.
Mismo stack y misma organización que APIzza, con un modelo de datos más completo: el stock se controla
**por talle**, hay un **carrito en sesión**, los pedidos tienen un **circuito de estados** y un **panel de reportes**.

> Documentos que acompañan al código: [`PROPUESTA.md`](PROPUESTA.md) (idea y alcance), [`BITACORA.md`](BITACORA.md)
> (seguimiento semanal) y [`GUIA_DEFENSA.md`](GUIA_DEFENSA.md) (qué explicar y dónde está cada cosa).

## Qué hace

| Rol | Qué puede hacer |
|---|---|
| **Cliente** | Ver el catálogo con filtros por categoría, talle y texto; ordenar y paginar; ver la ficha de cada modelo con su curva de talles; agregar pares al carrito; completar sus datos y confirmar el pedido; seguir el estado del pedido. |
| **Dueño del local** | Cargar, editar, pausar y eliminar modelos (con stock por talle); ver y buscar pedidos por estado; avanzar el estado (pendiente → pagado → enviado → entregado) o cancelar (repone el stock); ver clientes y su historial; mirar el panel con ventas, ranking de modelos y talles, ventas por categoría, stock bajo y modelos agotados. |

Las reglas que vale la pena recordar:

- **Nunca se vende lo que no hay.** El stock se valida al agregar al carrito, al mostrar el carrito y al confirmar.
  La confirmación descuenta el stock y guarda el pedido en una sola transacción (`SaveChangesAsync`): o se guarda todo o nada.
- **Dos personas no pueden comprar el último par.** `StockTalle.Cantidad` es un *token de concurrencia*: el `UPDATE`
  lleva `WHERE Cantidad = valor leído`; si otro pedido cambió el stock en el medio, EF Core lanza
  `DbUpdateConcurrencyException` y la app avisa en lugar de dejar stock negativo.
- **El precio queda congelado en el pedido.** `ItemsPedido.PrecioUnitario` y `Talle` guardan lo que se compró,
  aunque después el modelo cambie de precio o se elimine.
- **Los estados tienen reglas.** Solo se puede avanzar en orden; cancelar solo se puede desde *pendiente* o *pagado*
  y devuelve los pares al stock de cada talle. Un pedido entregado o cancelado no cambia más.
- **Un modelo que ya se vendió no se puede borrar** (FK con `RESTRICT`); se puede **pausar** y desaparece del catálogo.

## Estructura

```text
APIrJordan/
├── APIrJordan.sln
├── README.md · PROPUESTA.md · BITACORA.md · GUIA_DEFENSA.md
├── database/
│   ├── schema.sql                 # crea la BD apirjordan y las 5 tablas (con FK, UNIQUE y CHECK)
│   ├── inserts.sql                # 10 modelos, 100 filas de stock, 3 clientes y 4 pedidos de ejemplo
│   ├── consultas.sql              # 16 consultas: reportes del panel, controles y ejemplos de transacción
│   └── DER/                       # diagrama entidad-relación en PNG, SVG, Mermaid y Graphviz
└── APIrJordan.Web/
    ├── Program.cs                 # conexión MySQL, sesión, servicios y arranque MVC
    ├── appsettings.json           # cadena de conexión
    ├── Models/                    # Zapatilla, StockTalle, Cliente, Pedido, ItemPedido + Categorias, Talles, EstadoPedido
    ├── Data/                      # ApirJordanDbContext (mapeo y restricciones) + SeedData (datos de ejemplo)
    ├── Services/                  # CarritoService (carrito guardado en la sesión)
    ├── ViewModels/                # objetos a medida para cada vista (catálogo, carrito, checkout, panel...)
    ├── Helpers/                   # Formato (pesos, fechas) y ZapatillaImagen (ilustraciones)
    ├── Controllers/               # Home, Zapatillas, Carrito, Pedidos, Clientes, Panel
    ├── Views/                     # vistas Razor (.cshtml) + layout y parciales
    └── wwwroot/                   # css/site.css, js/site.js, images/zapatillas, fonts
```

## Preparar MySQL

Requiere **MySQL Server 8.x o superior** con el servicio iniciado. No hace falta XAMPP ni Docker.

1. Abrí MySQL Workbench, DBeaver o la consola de MySQL.
2. Ejecutá `database/schema.sql`. Crea la base `apirjordan` y las tablas `Zapatillas`, `StockTalles`, `Clientes`, `Pedidos` e `ItemsPedido`.
3. Ejecutá `database/inserts.sql`. Carga 10 modelos con su stock por talle, 3 clientes y 4 pedidos (uno por estado relevante).
4. Opcional: `database/consultas.sql` tiene los reportes y controles para probar desde el cliente SQL.

> `schema.sql` elimina y recrea las tablas. No lo vuelvas a ejecutar si ya tenés datos que quieras conservar.
>
> Si la app arranca contra una base vacía, `SeedData` crea las tablas y carga los mismos datos de ejemplo sola.
> Los scripts existen para poder revisar y defender la estructura sin depender del código.

## Conexión MySQL → página web

Archivo: `APIrJordan.Web/appsettings.json`

```text
Server=127.0.0.1;Port=3306;Database=apirjordan;User=root;Password=CAMBIAR_ESTO;SslMode=None;
```

- `Server=127.0.0.1`: MySQL está en tu propia computadora (equivale a `localhost`).
- `Port=3306`: puerto habitual del servidor MySQL.
- `Database=apirjordan`: base que crean los scripts SQL.
- `User=root` / `Password=...`: usuario y contraseña que elegiste al instalar MySQL.

La URL que muestra `dotnet run` (por ejemplo `http://localhost:5023`) es la de **ASP.NET**; la conexión a la base usa el `127.0.0.1:3306` de arriba.

## Ejecutar la página

Requiere [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
cd .\APIrJordan.Web
dotnet restore
dotnet build
dotnet run
```

Abrí la URL que muestre `dotnet run` (por defecto `http://localhost:5023`).

## Qué se guarda en MySQL

| Acción en la web | Tabla(s) |
|---|---|
| Cargar o editar un modelo (ficha + stock por talle) | `Zapatillas`, `StockTalles` |
| Pausar / eliminar un modelo | `Zapatillas` (`Activa = 0`) / `DELETE` en cascada sobre `StockTalles` |
| Agregar al carrito | nada: el carrito vive en la **sesión** hasta que se confirma |
| Confirmar la compra | `Clientes` (si el email es nuevo), `Pedidos`, `ItemsPedido` y `UPDATE` de `StockTalles` |
| Cambiar el estado de un pedido | `Pedidos.Estado` y `ActualizadoEn`; si se cancela, `UPDATE` de `StockTalles` |

## Páginas

| URL | Qué hace |
|---|---|
| `/` | Portada: destacados, categorías, novedades y cómo comprar |
| `/Zapatillas` | Catálogo con filtros (`?categoria=retro&talle=42&q=jordan&orden=precio_asc&pagina=2`) |
| `/Zapatillas/Detalles/1` | Ficha del modelo: talles con stock, selección de talle, relacionados |
| `/Zapatillas/Crear` · `/Zapatillas/Editar/1` | Alta y edición de un modelo con su stock por talle |
| `/Carrito` | Carrito: cambiar cantidades, quitar líneas, vaciar |
| `/Pedidos/Checkout` | Datos del comprador y confirmación |
| `/Pedidos/Confirmacion/5` | Comprobante del pedido recién creado |
| `/Pedidos` · `/Pedidos?estado=pendiente&q=valentina` | Lista de pedidos con pestañas por estado y búsqueda |
| `/Pedidos/Detalles/5` | Detalle, línea de tiempo del estado y botones para avanzarlo o cancelar |
| `/Clientes` · `/Clientes/Detalles/1` | Clientes con sus totales e historial de pedidos |
| `/Panel` | Reportes: ventas, pedidos por estado, ranking de modelos y talles, stock bajo |

## Datos de ejemplo

`inserts.sql` y `SeedData` cargan el mismo escenario:

- **10 modelos**: 5 retro (AJ1 High Chicago, AJ4 Bred, AJ11 Concord, AJ3 White Cement, AJ12 Taxi —agotada—),
  3 lifestyle (AJ1 Low Bred Toe, Spizike Low, Max Aura 6) y 2 de básquet (Luka 3, Tatum 3).
- **Curva de talles** 38 a 45 con medios puntos (40.5 y 42.5): 10 filas de stock por modelo.
- **3 clientes** y **4 pedidos**: uno entregado, uno enviado, uno cancelado y uno pendiente, para poder mostrar
  todo el circuito de estados y los reportes sin tener que cargar nada a mano.
