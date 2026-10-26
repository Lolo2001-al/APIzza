# Guía para la defensa

## 1. Código asincrónico (controladores)

Todas las acciones son `async Task<IActionResult>` y esperan la base con `await`.

```csharp
// Controllers/PizzasController.cs
public async Task<IActionResult> Detalles(int id)
{
    Pizza? pizza = await _contexto.Pizzas.FindAsync(id);   // await: espera la base
    ...
}
```

- `async` marca el método como asincrónico y `await` espera el resultado de una operación lenta (la base de datos).
- Mientras espera, el hilo del servidor **no queda bloqueado**: puede atender otros pedidos. Eso permite atender más usuarios a la vez.
- Ejemplos en el proyecto: `ToListAsync()`, `FindAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`.
- Mejores ejemplos para mostrar: `PedidosController.Crear` (POST) y `PizzasController.Index`.

## 2. MVC: las tres partes en este proyecto

| Parte | Qué es | Dónde está |
|-------|--------|------------|
| **Model** | Los datos y su estructura | `Models/` (Pizza, Cliente, Pedido, ItemPedido). `Data/ApizzaDbContext` los guarda en MySQL |
| **View** | Lo que ve el usuario (HTML + Razor) | `Views/` (`.cshtml`) |
| **Controller** | Recibe la petición, busca el Model y elige la vista | `Controllers/` |

**Flujo de una petición** (ej. `/Pizzas/Detalles/3`):

1. El navegador pide la URL. El **routing** (`Program.cs`, `{controller}/{action}/{id?}`) elige `PizzasController.Detalles(3)`.
2. El **controlador** le pide la pizza 3 a la base (Model).
3. El controlador hace `return View(pizza)`: le pasa el Model a la vista.
4. La **vista** `Views/Pizzas/Detalles.cshtml` genera el HTML con esos datos.
5. El HTML vuelve al navegador.

Otras piezas para mencionar:
- **ViewModel**: objeto armado a medida para una vista cuando el Model solo no alcanza (`MenuViewModel`, `PedidoViewModel`).
- **Inyección de dependencias**: el controlador recibe el `ApizzaDbContext` por el constructor; se registra en `Program.cs` con `AddDbContext`.

## 3. Cómo el controlador le pasa objetos a la vista

**En el controlador:** se pasa el objeto como argumento de `View(...)`.

```csharp
// HomeController.Index  -> pasa una lista de Pizza
List<Pizza> destacadas = await _contexto.Pizzas.Take(3).ToListAsync();
return View(destacadas);

// PizzasController.Index -> pasa un ViewModel con varias cosas juntas
return View(new MenuViewModel { Pizzas = ..., Categorias = ..., CategoriaActual = categoria });
```

**En la vista:** la directiva `@model` declara el tipo que se espera y se usa con `Model`.

```cshtml
@model List<Pizza>

@foreach (var pizza in Model)
{
    <partial name="_TarjetaPizza" model="pizza" />
}
```

- `@model` (minúscula) = el **tipo** del modelo. `Model` (mayúscula) = el **objeto** que mandó el controlador.
- La vista es fuertemente tipada: si se escribe mal una propiedad, da error al compilar.
- Para pasar solo un dato suelto existen `ViewBag` / `ViewData`, pero se prefiere un modelo tipado.

**Camino inverso (formulario → controlador):** en `Pedidos/Crear` el POST llega al controlador ya convertido en un `PedidoViewModel` (*model binding*), se valida con `ModelState.IsValid` (anotaciones `[Required]`, `[EmailAddress]`, `[Range]`) y, si todo está bien, se guarda y se redirige a `Detalles` (patrón Post/Redirect/Get, con `TempData` para el mensaje de confirmación).

## 4. Base de datos MySQL y persistencia

La aplicación usa Entity Framework Core con el proveedor oficial `MySql.EntityFrameworkCore`. La conexión está centralizada en `appsettings.json` y se registra en `Program.cs`.

El modelo relacional tiene cuatro tablas:

- `Pizzas`: catálogo del menú.
- `Clientes`: datos del comprador.
- `Pedidos`: cabecera del pedido.
- `ItemsPedido`: detalle con pizza, cantidad y precio unitario.

El flujo de un pedido es: formulario → `PedidoViewModel` → búsqueda/actualización de `Cliente` → creación de `Pedido` → creación de `ItemPedido` → `SaveChangesAsync()` → detalle del pedido.

La carga de nuevas pizzas usa `PizzasController.Crear`, crea un registro en `Pizzas` y redirige a la categoría elegida.

## 5. Qué se cambió respecto de la versión anterior (Web API)

- El proyecto era una **API REST** que devolvía JSON y una landing aparte en JS con `fetch`. Ahora es **MVC con vistas Razor**: el servidor genera el HTML.
- Se sacó lo que no pide esta entrega: Swagger, CORS, DTOs, y los endpoints PUT / DELETE / PATCH.
- La integración con la API queda para el 4° bimestre.

## 6. Preguntas que pueden hacer

- **¿Qué pasa si no uso `await`?** Se obtiene un `Task` sin resolver y el código sigue sin esperar el resultado; los datos no están listos.
- **¿Diferencia entre `Controller` y `ControllerBase`?** `Controller` agrega soporte para vistas (`View()`); `ControllerBase` es para APIs.
- **¿Para qué sirve `_Layout.cshtml`?** Plantilla común (barra y pie); `@RenderBody()` es donde se inserta cada vista.
- **¿Por qué `ValidateAntiForgeryToken`?** Protege el formulario POST contra CSRF.
- **¿Por qué el precio se lee de la base y no del formulario?** Porque el navegador puede manipular lo que envía.
