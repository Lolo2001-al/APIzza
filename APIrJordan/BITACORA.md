# Bitácora del proyecto — APIr Jordan

**Materia:** Programación / Base de datos · **Proyecto:** tienda de zapatillas Jordan (ASP.NET Core MVC + EF Core + MySQL)
**Período:** del 14/10/2026 a la anteúltima semana de noviembre (16 al 22/11/2026) · **Defensa:** última semana de noviembre

> Nota: la bitácora está armada siguiendo el cronograma de `PROPUESTA.md` (un hito por semana, seguimientos los viernes).
> Si en la cursada cambia una fecha o surge algo distinto, se corrige la entrada de esa semana: la idea es que refleje lo que
> realmente pasó, no lo que estaba planeado.

Cada entrada tiene el mismo formato: **qué hice**, **problemas o decisiones** y **próximo paso**. Al final hay un resumen por hito
y la lista de mejoras que quedaron afuera.

---

## Semana 1 · 14 al 18 de octubre — Elección del tema y diseño

### Miércoles 14/10/2026 · Elección del tema y propuesta (3 h)

**Qué hice**
- Releí la consigna y separé lo obligatorio (objetivo, entregables, hitos de seguimiento, matriz de evaluación) de lo deseable.
- Elegí el tema: una tienda de zapatillas Jordan. Lo pensé a partir de APIzza (pizzería en MVC que ya conocía) para no arrancar de cero
  con el stack, pero buscando un problema que justifique una base de datos más completa.
- Escribí `PROPUESTA.md` con objetivo general y específicos, alcance, entregables, cronograma y cómo apunta cada parte a la matriz de evaluación.

**Problemas o decisiones**
- La decisión más importante: el stock se controla **por talle**. En una zapatillería no alcanza con "hay 12 pares", hay que saber cuántos
  hay del 42. Eso agrega una tabla (`StockTalles`) y es lo que diferencia al proyecto de una pizzería con otro nombre.
- Descarté hacer login/roles en esta entrega: suma tiempo y no está en lo obligatorio. Queda anotado como mejora.

**Próximo paso:** dibujar el DER y definir las columnas de cada tabla.

### Viernes 16/10/2026 · DER y estructura del proyecto (2 h)

**Qué hice**
- Diagrama entidad-relación con 5 tablas: `Zapatillas`, `StockTalles`, `Clientes`, `Pedidos`, `ItemsPedido` (`database/DER/`).
- Definí tipos y restricciones: `Talle DECIMAL(3,1)` (hay medios puntos: 40.5, 42.5), `UNIQUE (ZapatillaId, Talle)`, `CHECK (Cantidad >= 0)`,
  `Email` único en clientes, precio histórico en `ItemsPedido`.
- Creé la solución `APIrJordan.sln` y el proyecto web vacío con la misma estructura de carpetas de APIzza (Models, Data, Controllers, Views, wwwroot).
- Elegí la paleta y las tipografías: fondo claro, tinta casi negra, un solo color de acento (rojo Chicago). Minimalista, con animaciones suaves.

**Problemas o decisiones**
- Dudé entre guardar el talle como texto ("40.5") o número. Número: permite ordenar y comparar; el texto se formatea en la vista.
- `ItemsPedido → Zapatillas` con `ON DELETE RESTRICT`: no se puede borrar un modelo que ya se vendió. Para sacarlo del catálogo se **pausa** (`Activa = 0`).

**Próximo paso:** escribir `schema.sql` e `inserts.sql` y probarlos en MySQL.

---

## Semana 2 · 19 al 25 de octubre — Hito 1: base de datos

### Martes 20/10/2026 · schema.sql e inserts.sql (3 h)

**Qué hice**
- `database/schema.sql`: creación de la base `apirjordan`, las 5 tablas, índices, claves foráneas (agregadas con `ALTER TABLE` aparte, como en APIzza)
  y restricciones `CHECK`.
- `database/inserts.sql`: 10 modelos (5 retro, 3 lifestyle, 2 básquet), 100 filas de stock (10 talles × 10 modelos, con ceros incluidos),
  3 clientes y 4 pedidos, uno por cada estado que quiero mostrar (entregado, enviado, cancelado, pendiente).
- Ejecuté los dos scripts completos en Workbench y revisé las tablas.

**Problemas o decisiones**
- Al principio solo cargaba los talles con stock. Lo cambié a **todas** las filas de la curva (38 a 45) para que la ficha del modelo muestre
  los talles agotados en gris en vez de no mostrarlos.
- Las cantidades de stock ya tienen descontadas las ventas de los pedidos de ejemplo, para que los números cierren desde el día uno.
- Las fechas usan `NOW() - INTERVAL n DAY` para que "hace 3 días" siga siendo verdad el día de la defensa.

**Próximo paso:** consultas de reportes y el `DbContext`.

### Jueves 22/10/2026 · consultas.sql y DbContext (2 h)

**Qué hice**
- `database/consultas.sql` con 16 consultas: catálogo con stock total, curva de talles de un modelo, filtro por talle, pedidos con cliente (JOIN),
  detalle de pedido, KPIs, pedidos por estado, ranking de modelos y de talles, ventas por categoría, stock bajo, agotados (`NOT EXISTS`),
  clientes con totales, control de integridad de totales (`HAVING`) y dos bloques de transacción con `ROLLBACK`.
- `Data/ApirJordanDbContext.cs`: `DbSet` por tabla y `OnModelCreating` con las mismas restricciones que el SQL
  (longitudes, precisión decimal, índice único, comportamiento al borrar).

**Problemas o decisiones**
- Puse `Cantidad` de `StockTalles` como **token de concurrencia** (`IsConcurrencyToken`). Es una línea, pero resuelve el problema de dos personas
  comprando el último par al mismo tiempo. Lo voy a explicar en la defensa con el `UPDATE ... WHERE Cantidad = valor leído`.
- Los nombres de tablas del SQL y de EF tienen que coincidir exactamente (en Linux MySQL distingue mayúsculas). Los dejé iguales a los `DbSet`.

**Próximo paso:** seguimiento del viernes con la base lista.

### Viernes 23/10/2026 · Seguimiento — Hito 1 (1 h)

**Qué mostré:** DER, `schema.sql`, `inserts.sql` y las consultas corriendo en MySQL. Expliqué por qué el stock va en una tabla aparte y qué pasa al borrar en cada FK.

**Devolución / pendientes anotados:** que las consultas del panel de la app y las del archivo SQL den los mismos números (para poder verificar en la defensa);
tener un ejemplo de pedido cancelado para mostrar que el stock vuelve.

**Próximo paso:** modelos en C#, `SeedData` y las primeras pantallas de solo lectura.

---

## Semana 3 · 26 de octubre al 1 de noviembre — Hito 2: MVC de lectura

### Martes 27/10/2026 · Modelos, seed y conexión (3 h)

**Qué hice**
- Clases en `Models/`: `Zapatilla`, `StockTalle`, `Cliente`, `Pedido`, `ItemPedido`, más las "tablas chicas" en código: `Categorias`, `Talles`, `EstadoPedido`.
- `Data/SeedData.cs`: carga el mismo escenario que `inserts.sql` si la base está vacía (`EnsureCreated` + `Any()`).
- `Program.cs`: `AddDbContext` con `UseMySQL`, cadena de conexión en `appsettings.json`, routing por defecto.
- Primer `dotnet run` conectado a MySQL: la portada muestra la cantidad de modelos leída de la base.

**Problemas o decisiones**
- Error de conexión al principio: tenía `Database=apizza` copiado del otro proyecto. Lo cambié a `apirjordan`.
- Propiedades calculadas en los modelos (`StockTotal`, `Agotada`, `NombreCompleto`, `Subtotal`) marcadas como `=>`: EF no las mapea y las vistas quedan más limpias.

**Próximo paso:** catálogo y ficha del modelo.

### Jueves 29/10/2026 · Catálogo con filtros, orden y paginación (4 h)

**Qué hice**
- `ZapatillasController.Index` con `IQueryable`: filtro por categoría, por talle con stock (`Talles.Any(t => t.Talle == x && t.Cantidad > 0)`),
  búsqueda por nombre/colorway, orden (novedades, precio, nombre) y paginación de 8 por página. Todo con `async`/`await`.
- `CatalogoViewModel` con los filtros aplicados, total de resultados y páginas; vista con pestañas por categoría y chips de talle.
- `Detalles`: ficha con la curva de talles (radio buttons; agotados deshabilitados; "últimos pares" cuando quedan 1 o 2) y modelos relacionados.
- Portada (`HomeController`): destacadas, novedades, conteo por categoría.
- Primera versión de `site.css` y `site.js`: tarjetas que aparecen en cascada con `IntersectionObserver`.

**Problemas o decisiones**
- El talle viaja por la URL como `40.5` (punto). Puse la cultura invariante en `Program.cs` para que el *model binding* no lo lea como 405.
- Paginación: calculo el total con `CountAsync()` **antes** de `Skip/Take`; si no, el total era siempre el de la página.

**Próximo paso:** seguimiento del viernes.

### Viernes 30/10/2026 · Seguimiento — Hito 2 (1 h)

**Qué mostré:** portada, catálogo con filtros combinados (`?categoria=retro&talle=42&orden=precio-asc`), ficha de un modelo y la de la AJ12 "Taxi" agotada.
Expliqué MVC con el flujo de una URL y qué es un ViewModel.

**Devolución / pendientes anotados:** cuidar que la ficha funcione sin JavaScript; mostrar en pantalla cuántos talles tienen stock.

**Próximo paso:** alta/edición de modelos y carrito.

---

## Semana 4 · 2 al 8 de noviembre — Hito 3: alta/edición y carrito

### Martes 3/11/2026 · Alta, edición, pausar y eliminar (4 h)

**Qué hice**
- `ZapatillaFormViewModel` con Data Annotations y una lista de 10 `StockTalleFormViewModel` (una celda por talle).
- `Crear` y `Editar` comparten el parcial `_CamposZapatilla` (ficha, grilla de stock por talle, publicación con vista previa de la ilustración).
- `Pausar` (cambia `Activa`) y `Eliminar` (solo si nunca se vendió; si no, avisa y sugiere pausar). Ambos por POST con confirmación.
- Contador de "total cargado" en vivo con JS mientras se escriben las cantidades.

**Problemas o decisiones**
- La lista de talles del formulario tiene que postearse con índice (`Stock[3].Cantidad`) para que el *model binding* arme la lista. Con `asp-for` dentro del `for` sale solo.
- En `Editar` el stock se actualiza fila por fila (busco la fila del talle y cambio `Cantidad`) en lugar de borrar y volver a insertar: así no se pierde el `Id` y las FK siguen válidas.

**Próximo paso:** carrito en sesión.

### Jueves 5/11/2026 · Carrito en sesión (3 h)

**Qué hice**
- `Services/CarritoService.cs`: lista de `{ZapatillaId, Talle, Cantidad}` guardada en la sesión como JSON; `Agregar`, `Actualizar`, `Quitar`, `Vaciar`
  y `ArmarResumenAsync`, que vuelve a leer la base y marca las líneas con problemas (modelo pausado, sin stock, stock menor al pedido).
- `CarritoController` y la vista: steppers de cantidad que envían el formulario solos, botón de respaldo en `<noscript>`, resumen con total.
- Contador de pares en el header con un "latido" cuando cambia, y toasts para los mensajes de `TempData`.
- `Program.cs`: `AddSession`, `UseSession`, `AddHttpContextAccessor`, `AddScoped<CarritoService>`.

**Problemas o decisiones**
- Máximo 5 pares por línea y nunca más que el stock del talle. Se valida al agregar **y** al confirmar, porque el stock puede cambiar en el medio.
- El carrito no toca la base a propósito: es un borrador. Recién al confirmar se descuenta el stock.

**Próximo paso:** seguimiento del viernes.

### Viernes 6/11/2026 · Seguimiento — Hito 3 (1 h)

**Qué mostré:** cargar un modelo nuevo con stock por talle, editarlo, pausarlo; agregar pares de distintos talles al carrito, cambiar cantidades, ver el aviso cuando el stock no alcanza.

**Devolución / pendientes anotados:** que el checkout vuelva a verificar stock y precios desde la base; agregar número de pedido visible en la confirmación.

**Próximo paso:** checkout con transacción y estados del pedido.

---

## Semana 5 · 9 al 15 de noviembre — Hito 4: checkout, estados y panel

### Martes 10/11/2026 · Checkout: transacción y concurrencia (4 h)

**Qué hice**
- `PedidosController.Checkout` (GET y POST): formulario con `CheckoutViewModel` (nombre, email, teléfono, dirección, nota) y resumen al costado.
- Al confirmar: busca o crea el `Cliente` por email, arma `Pedido` + `ItemsPedido` con el precio de la base, descuenta `StockTalles`
  y hace **un solo** `SaveChangesAsync()`. Después `RedirectToAction` a `Confirmacion` (Post/Redirect/Get).
- `catch (DbUpdateConcurrencyException)`: si otra persona compró en el medio, avisa y vuelve al carrito.
- Probé la concurrencia con dos navegadores comprando el último par del 42: uno compra, el otro recibe el aviso. Stock nunca negativo.

**Problemas o decisiones**
- Al principio descontaba el stock en una consulta aparte y después guardaba el pedido: dos `SaveChanges`, dos transacciones. Lo junté en uno para que sea todo o nada.
- Las líneas del pedido **no** se postean desde el formulario: se vuelven a armar desde la sesión + la base. Lo que manda el navegador solo son los datos del comprador.

**Próximo paso:** circuito de estados.

### Jueves 12/11/2026 · Estados del pedido, cancelación y gestión (3 h)

**Qué hice**
- `EstadoPedido.Siguientes(estado)` y `PuedePasarA`: pendiente → pagado → enviado → entregado; cancelar solo desde pendiente o pagado.
- `CambiarEstado` (POST): valida la transición en el servidor; si es cancelación, devuelve los pares al stock de cada talle en la misma operación.
- `Pedidos/Index` con pestañas por estado (con conteos) y búsqueda por cliente, email o número; `Pedidos/Detalles` con línea de tiempo animada y los botones
  generados a partir de `Siguientes`.
- `ClientesController`: lista con cantidad de pedidos y total gastado, y detalle con historial.

**Problemas o decisiones**
- Los botones de la vista salen de la misma función que valida en el servidor; así la pantalla y la regla no se pueden contradecir.
- `ActualizadoEn` se guarda en cada cambio de estado para mostrar "último cambio: hace 2 días".

**Próximo paso:** panel de reportes y seguimiento.

### Viernes 13/11/2026 · Panel y seguimiento — Hito 4 (3 h)

**Qué hice**
- `PanelController`: ventas totales, ticket promedio, pedidos por estado, pares vendidos y en stock, modelos agotados, ranking de modelos y de talles,
  ventas por categoría, stock bajo y últimos pedidos. Los cancelados no cuentan como venta.
- Vista con contadores animados y barras de ranking que crecen al entrar en pantalla.
- Comparé cada número del panel con las consultas 6 a 12 de `consultas.sql`: coinciden.

**Qué mostré en el seguimiento:** una compra completa de punta a punta, el pedido avanzando de estado, una cancelación que repone stock y el panel.

**Devolución / pendientes anotados:** páginas de error propias; revisar textos y responsive en el celular; terminar documentación y preparar la defensa.

---

## Semana 6 · 16 al 22 de noviembre (anteúltima) — Hito 5: pulido, documentación y ensayo

### Martes 17/11/2026 · Pulido de diseño y detalles (3 h)

**Qué hice**
- Páginas de error (`/Home/Error`, 404 con `UseStatusCodePagesWithReExecute`) con el mismo diseño del sitio.
- Repaso responsive: grillas a una columna en el celular, tablas con scroll horizontal, header con el carrito siempre visible.
- Animaciones revisadas una por una: aparición en cascada, latido del carrito, barras del panel, línea de tiempo, toasts.
  Se desactivan con `prefers-reduced-motion`.
- Accesibilidad básica: `aria-label` en botones de ícono, foco visible, `<noscript>` donde hacía falta.

**Problemas o decisiones**
- Saqué dos animaciones que distraían (fondo de la portada y hover de las tablas). Minimalista también es sacar.

**Próximo paso:** documentación.

### Jueves 19/11/2026 · Documentación y ensayo de la defensa (3 h)

**Qué hice**
- `README.md` (instalación, conexión, páginas, qué se guarda en cada tabla), `GUIA_DEFENSA.md` (qué explicar y dónde está cada cosa, preguntas probables)
  y revisión final de `PROPUESTA.md` para que coincida con lo entregado.
- Ensayo de la defensa con cronómetro: 2 min de problema y base, 4 min de demo (compra completa + gestión + panel), 2 min de transacción/concurrencia, 2 min de preguntas.
- Base recreada desde cero con los scripts para confirmar que la instalación documentada funciona en una máquina limpia.

**Problemas o decisiones**
- La demo se hace con la base de ejemplo (`inserts.sql`) y un pedido nuevo en vivo. Si falla MySQL, tengo capturas de respaldo de cada pantalla.

**Próximo paso:** cierre y entrega.

### Viernes 20/11/2026 · Cierre — entrega final (1 h)

**Qué hice**
- Revisión final de la lista de entregables contra la consigna: código, scripts SQL, DER, README, guía de defensa, propuesta y esta bitácora.
- Último `dotnet build` sin advertencias y prueba completa del flujo.
- Entrega del repositorio y confirmación del turno de defensa para la semana del 23 al 27 de noviembre.

---

## Resumen por hito

| Hito | Semana | Entregable | Estado |
|---|---|---|---|
| H0 · Tema y propuesta | 14–18/10 | `PROPUESTA.md`, DER, estructura del proyecto | ✔ |
| H1 · Base de datos | 19–25/10 | `schema.sql`, `inserts.sql`, `consultas.sql`, `DbContext` | ✔ |
| H2 · MVC de lectura | 26/10–1/11 | portada, catálogo con filtros/orden/paginación, ficha | ✔ |
| H3 · Alta/edición y carrito | 2–8/11 | formularios con stock por talle, pausar/eliminar, carrito en sesión | ✔ |
| H4 · Checkout, estados y panel | 9–15/11 | transacción + concurrencia, circuito de estados, clientes, panel | ✔ |
| H5 · Pulido y documentación | 16–22/11 | errores, responsive, animaciones, README, guía, ensayo | ✔ |
| Defensa | 23–27/11 | presentación de 10 min + preguntas | pendiente |

**Horas totales aproximadas:** 50 h (unas 8 por semana).

## Mejoras que quedaron afuera (para mencionar en la defensa)

1. Login con roles (cliente / dueño) para proteger la gestión y el panel.
2. Pago online y envío de email al cambiar el estado del pedido.
3. Subida de fotos reales desde el formulario (hoy se elige entre ilustraciones incluidas).
4. Reserva temporal de stock mientras el cliente está en el checkout.
5. Exportar los reportes del panel a CSV.
