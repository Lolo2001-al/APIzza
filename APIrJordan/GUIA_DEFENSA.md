# Guía para la defensa — APIr Jordan

Qué explicar, en qué archivo está y qué responder si preguntan. Está ordenada como una defensa de 10 minutos:
primero el problema y la base, después el flujo de una compra, después los detalles técnicos.

## 0. El proyecto en 30 segundos

> "APIr Jordan es una tienda de zapatillas Jordan hecha en ASP.NET Core MVC con Entity Framework Core y MySQL.
> El problema que resuelve es el stock **por talle**: en una zapatillería no sirve saber que hay 12 pares de un modelo,
> hay que saber cuántos hay del 42. Por eso la base tiene una tabla `StockTalles` con una fila por modelo y talle,
> y toda la lógica (catálogo, carrito, confirmación, cancelación) gira alrededor de no vender nunca un par que no existe."

Datos para tener a mano: 5 tablas, 6 controladores, 14 pantallas (más layout y 3 parciales), 10 modelos de ejemplo con 100 filas de stock, 4 pedidos de ejemplo.

## 1. Base de datos (`database/`, `Data/ApirJordanDbContext.cs`)

| Tabla | Para qué | Detalle que vale puntos |
|---|---|---|
| `Zapatillas` | el catálogo (modelo + colorway) | `Activa` permite **pausar** sin borrar; `Destacada` arma la portada |
| `StockTalles` | **una fila por (modelo, talle)** | `UNIQUE (ZapatillaId, Talle)`, `CHECK (Cantidad >= 0)`, `Talle DECIMAL(3,1)` para medios puntos (40.5) |
| `Clientes` | quién compra | `Email` único: si vuelve a comprar se reutiliza el mismo cliente |
| `Pedidos` | cabecera de la compra | `Estado` con circuito, `Total` calculado en el servidor, `ActualizadoEn` para la línea de tiempo |
| `ItemsPedido` | líneas de la compra | guarda `Talle` y `PrecioUnitario` **históricos**: el pedido no cambia si el modelo cambia de precio |

Claves foráneas y qué pasa al borrar:

- `StockTalles → Zapatillas`: **CASCADE** (se borra el modelo, se borra su stock).
- `ItemsPedido → Pedidos`: **CASCADE** (las líneas no tienen sentido sin el pedido).
- `ItemsPedido → Zapatillas`: **RESTRICT** (no se puede borrar un modelo que ya se vendió → se pausa).
- `Pedidos → Clientes`: **RESTRICT** (no se puede borrar un cliente con pedidos).

Todo esto está dos veces, a propósito: en `schema.sql` (para mostrarlo en SQL) y en `OnModelCreating` del `DbContext`
(para que EF Core lo conozca). El DER está en `database/DER/`.

## 2. MVC: las tres partes en este proyecto

| Parte | Qué es | Dónde está |
|---|---|---|
| **Model** | Los datos y su estructura | `Models/` (Zapatilla, StockTalle, Cliente, Pedido, ItemPedido). `Data/ApirJordanDbContext` los guarda en MySQL |
| **View** | Lo que ve el usuario (HTML + Razor) | `Views/` (`.cshtml`), con `_Layout.cshtml` como marco común y parciales (`_TarjetaZapatilla`, `_Estado`) |
| **Controller** | Recibe la petición, consulta el Model y elige la vista | `Controllers/` (Home, Zapatillas, Carrito, Pedidos, Clientes, Panel) |

**Flujo de una petición** (ej. `/Zapatillas/Detalles/3?talle=42`):

1. El navegador pide la URL. El **routing** de `Program.cs` (`{controller=Home}/{action=Index}/{id?}`) elige `ZapatillasController.Detalles(3, 42)`.
2. El controlador busca la zapatilla 3 con sus talles (`Include(z => z.Talles)`) y arma un `ZapatillaDetalleViewModel`.
3. `return View(modelo)`: le pasa el objeto a la vista.
4. `Views/Zapatillas/Detalles.cshtml` genera el HTML (talles disponibles, precio, botón de compra).
5. El HTML vuelve al navegador.

Otras piezas para nombrar:

- **ViewModel** (`ViewModels/`): objeto armado a medida para una vista cuando el Model solo no alcanza. Ej.: `CatalogoViewModel`
  lleva la lista de zapatillas **y** los filtros aplicados, el total de resultados y la paginación.
- **Inyección de dependencias**: los controladores reciben `ApirJordanDbContext` y `CarritoService` por el constructor;
  se registran en `Program.cs` con `AddDbContext` y `AddScoped`.
- **Servicio** (`Services/CarritoService.cs`): lógica que usan varios controladores (Zapatillas, Carrito, Pedidos y el layout)
  y que no es de ningún modelo en particular.
- **Helpers** (`Helpers/`): formateo de pesos y fechas en es-AR, resolución de imágenes. Evitan repetir código en las vistas.

## 3. El flujo de una compra (para mostrar en vivo)

1. **Catálogo** `/Zapatillas?talle=42`: el filtro por talle es una consulta sobre `StockTalles`
   (`z.Talles.Any(t => t.Talle == 42 && t.Cantidad > 0)`), por eso solo aparecen modelos que de verdad tienen ese talle.
2. **Ficha** `/Zapatillas/Detalles/1`: cada talle es un radio button; los agotados están deshabilitados y los de 1–2 pares dicen "últimos pares".
3. **Agregar al carrito** (`CarritoController.Agregar`): valida talle, stock y el máximo de 5 pares por línea; guarda `{ZapatillaId, Talle, Cantidad}` en la sesión.
   No toca la base: el stock se descuenta recién al confirmar.
4. **Carrito** `/Carrito`: `ArmarResumenAsync` vuelve a leer la base y marca problemas (modelo pausado, sin stock, stock menor al pedido).
5. **Checkout** `/Pedidos/Checkout`: datos del comprador con validaciones (`[Required]`, `[EmailAddress]`, `[StringLength]`).
6. **Confirmar** (`PedidosController.Checkout` POST): crea o reutiliza el `Cliente`, arma `Pedido` + `ItemsPedido`, descuenta `StockTalles`
   y hace **un solo** `SaveChangesAsync()`. Redirige al comprobante (patrón **Post/Redirect/Get**: recargar no duplica el pedido).
7. **Gestión** `/Pedidos/Detalles/5`: línea de tiempo y botones "Marcar pagado", "Marcar enviado", "Marcar entregado" o "Cancelar".

## 4. Transacción y concurrencia (la parte más "difícil", explicada simple)

```csharp
// Controllers/PedidosController.cs (Checkout POST)
foreach (var linea in carrito.Lineas)
{
    var stock = linea.Zapatilla.Talles.First(t => t.Talle == linea.Talle);
    stock.Cantidad -= linea.Cantidad;                       // descuento del talle
    pedido.Items.Add(new ItemPedido { ..., PrecioUnitario = linea.Zapatilla.Precio });
}
pedido.Total = pedido.Items.Sum(i => i.Subtotal);
_contexto.Pedidos.Add(pedido);

try
{
    await _contexto.SaveChangesAsync();                     // cliente + pedido + items + stock: todo o nada
}
catch (DbUpdateConcurrencyException)
{
    TempData["Error"] = "Justo se vendió alguno de los pares que elegiste...";
    return RedirectToAction("Index", "Carrito");
}
```

- **Transacción**: EF Core envuelve todos los cambios de un `SaveChangesAsync` en una transacción de MySQL.
  Si falla el `INSERT` del pedido, tampoco queda el `UPDATE` del stock.
- **Concurrencia optimista**: en `ApirJordanDbContext`, `Cantidad` está marcada con `IsConcurrencyToken()`.
  El `UPDATE` que genera EF es `UPDATE StockTalles SET Cantidad = 2 WHERE Id = 7 AND Cantidad = 3`.
  Si otra persona compró en el medio, `Cantidad` ya no es 3, el `UPDATE` afecta 0 filas y EF lanza la excepción.
  Resultado: dos personas no pueden comprar el último par. En `consultas.sql` (consulta 15) está la misma idea escrita en SQL a mano.
- **Cancelación** (`CambiarEstado` → `cancelado`): recorre los items y hace `stock.Cantidad += item.Cantidad`,
  también en un solo `SaveChangesAsync`.

## 5. Estados del pedido (`Models/EstadoPedido.cs`)

```text
pendiente → pagado → enviado → entregado
    └──────────┴──► cancelado (repone el stock)
```

- `Siguientes(estado)` devuelve a qué estados se puede pasar; `PuedePasarA(actual, nuevo)` lo valida en el servidor.
  Aunque alguien arme el POST a mano, un pedido *enviado* no puede volver a *pendiente* ni cancelarse.
- Los botones de la vista se generan a partir de `Siguientes`, así la pantalla y la regla nunca se contradicen.
- La línea de tiempo de `Pedidos/Detalles` usa `Posicion(estado)` (0 a 3) para pintar el progreso.

## 6. Código asincrónico

Todas las acciones que tocan la base son `async Task<IActionResult>` y usan `await`:

```csharp
public async Task<IActionResult> Index(string? categoria, decimal? talle, string? q, string? orden, int pagina = 1)
{
    IQueryable<Zapatilla> consulta = _contexto.Zapatillas.Where(z => z.Activa);
    if (categoria != null) consulta = consulta.Where(z => z.Categoria == categoria);
    ...
    var total = await consulta.CountAsync();
    var zapatillas = await consulta.Include(z => z.Talles).Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
}
```

- Mientras MySQL responde, el hilo del servidor **no queda bloqueado** y puede atender a otros usuarios.
- `IQueryable` permite **armar la consulta por partes** (filtros, orden, paginación) y recién al final (`ToListAsync`) se ejecuta un solo `SELECT` con todo.
- Ejemplos: `ToListAsync()`, `CountAsync()`, `SumAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`.

## 7. Sesión y carrito (`Services/CarritoService.cs`, `Program.cs`)

- `AddSession` + `UseSession` en `Program.cs`. La sesión guarda solo una lista chica de `{ZapatillaId, Talle, Cantidad}` serializada en JSON.
- El servicio se registra con `AddScoped` (una instancia por petición) y recibe `IHttpContextAccessor` para llegar a la sesión.
- Por qué en sesión y no en la base: el carrito es un borrador; no tiene sentido guardar pedidos que nunca se confirman.
  Pasa a la base solo cuando el usuario confirma.

## 8. Validaciones (tres capas)

1. **Navegador**: `required`, `min`, `max`, tipos de input y los steppers de `site.js` (cantidad entre 1 y el stock del talle).
2. **Servidor**: Data Annotations en los ViewModels (`ZapatillaFormViewModel`, `CheckoutViewModel`) + `ModelState.IsValid`,
   más reglas de negocio en el código (talle válido, stock suficiente, transición de estado permitida, máximo por línea).
3. **Base de datos**: `NOT NULL`, `UNIQUE`, `CHECK (Cantidad >= 0)`, `CHECK (Estado IN (...))`, claves foráneas.

Si preguntan "¿por qué repetir la validación?": porque el navegador se puede saltear (formulario a mano, JavaScript apagado)
y la base es la última barrera ante un error de programación.

## 9. Seguridad básica que sí está

- `[ValidateAntiForgeryToken]` en todos los POST; los formularios Razor agregan el token solos.
- Todas las operaciones que cambian datos son **POST** (agregar al carrito, cambiar estado, pausar, eliminar); nunca un link GET.
- Los precios y totales se toman **siempre de la base**, nunca del formulario.
- `TempData` + `RedirectToAction` después de cada POST (Post/Redirect/Get).
- Páginas de error propias (`/Home/Error`, 404 con `UseStatusCodePagesWithReExecute`).

Lo que **no** tiene y conviene decirlo antes de que pregunten: login/roles (el panel y la gestión están abiertos) y pago real.
Son el siguiente paso natural y están anotados en la bitácora como mejoras pendientes.

## 10. Front: minimalista con animaciones (`wwwroot/css/site.css`, `wwwroot/js/site.js`)

- CSS propio, sin frameworks. Variables de diseño (`--tinta`, `--acento`, `--linea`), grillas con `grid`, tipografías locales (Inter + Space Grotesk).
- Animaciones con sentido: aparición en cascada de las tarjetas (`IntersectionObserver` + clase `.revelar`), contadores del panel,
  barras de ranking que crecen, latido del contador del carrito, línea de tiempo del pedido, toasts de confirmación.
- Todo funciona **sin JavaScript** (los formularios se envían igual; el carrito tiene un botón de respaldo en `<noscript>`); el JS solo mejora la experiencia.
- `prefers-reduced-motion` desactiva las animaciones si el sistema lo pide.

## 11. Preguntas que pueden hacer (respuesta corta)

- **¿Por qué una tabla aparte para el stock?** Porque la unidad que se vende es (modelo, talle). Una columna `Stock` en `Zapatillas` no sabe cuántos 42 hay.
- **¿Qué pasa si dos personas compran el último par a la vez?** Token de concurrencia en `Cantidad`: el segundo `UPDATE` no afecta filas, EF lanza `DbUpdateConcurrencyException` y se avisa al usuario. Nunca queda stock negativo (y además hay `CHECK` en la base).
- **¿Por qué `PrecioUnitario` en `ItemsPedido` si ya está en `Zapatillas`?** Histórico: si mañana sube el precio, el pedido de ayer no cambia.
- **¿Por qué `DECIMAL(3,1)` para el talle?** Porque existen talles con medio punto (40.5, 42.5) y hace falta ordenarlos y compararlos como números.
- **¿Qué diferencia hay entre pausar y eliminar?** Pausar (`Activa = 0`) saca el modelo del catálogo sin perder historial. Eliminar solo se permite si nunca se vendió (FK `RESTRICT`).
- **¿Dónde está el patrón Post/Redirect/Get?** En todos los POST: `Checkout`, `CambiarEstado`, `Carrito/Agregar`, `Crear`, `Editar`, etc. Después de guardar, `RedirectToAction`.
- **¿Qué es un ViewModel y por qué no usar el Model directo?** Es un objeto hecho para la pantalla. `CheckoutViewModel` tiene los campos del formulario y el resumen; `Pedido` tiene lo que se guarda. Separarlos evita que un formulario pueda sobreescribir campos que no debe (`Total`, `Estado`).
- **¿Cómo se conecta con MySQL?** `appsettings.json` → `ConnectionStrings:ApirJordanDb` → `Program.cs` `UseMySQL(...)` → `ApirJordanDbContext`. Proveedor `MySql.EntityFrameworkCore`.
- **¿Qué hace `Include`?** Trae la relación en la misma consulta (`JOIN`): `Zapatillas` con sus `Talles`, `Pedidos` con `Cliente` e `Items`. Sin `Include`, las listas vendrían vacías.
- **¿Qué muestra el panel y de dónde sale?** `PanelController` calcula ventas, pedidos por estado, ranking de modelos y talles, ventas por categoría, stock bajo y agotados con LINQ; las mismas cuentas están en `consultas.sql` (6 a 12) para verificar contra la base.
- **¿Qué mejorarías?** Login con roles (cliente / dueño), pago online, fotos reales subidas desde el formulario, envío de email al cambiar el estado, reservar stock por unos minutos mientras se está en el checkout.
