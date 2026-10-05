using APIzza.Web.Data;
using APIzza.Web.Models;
using APIzza.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIzza.Web.Controllers;

/// <summary>Crear y consultar pedidos.</summary>
public class PedidosController : Controller
{
    private readonly ApizzaDbContext _contexto;

    public PedidosController(ApizzaDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /Pedidos  -> lista de pedidos, del mas nuevo al mas viejo
    public async Task<IActionResult> Index()
    {
        List<Pedido> pedidos = await _contexto.Pedidos
            .Include(p => p.Cliente)
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return View(pedidos);
    }

    // GET /Pedidos/Detalles/5
    public async Task<IActionResult> Detalles(int id)
    {
        Pedido? pedido = await _contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Items)
                .ThenInclude(i => i.Pizza)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    // GET /Pedidos/Crear  (o /Pedidos/Crear?pizzaId=2 para venir con una pizza elegida)
    public async Task<IActionResult> Crear(int? pizzaId)
    {
        var pizzas = await _contexto.Pizzas.OrderBy(p => p.Id).ToListAsync();

        // Convertimos el Model (Pizza) en lineas del formulario (ViewModel)
        var modelo = new PedidoViewModel
        {
            Lineas = pizzas.Select(p => new LineaPedidoViewModel
            {
                PizzaId = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Cantidad = p.Id == pizzaId ? 1 : 0,
            }).ToList(),
        };

        return View(modelo);
    }

    // POST /Pedidos/Crear  -> el formulario llega ya convertido en un PedidoViewModel (model binding)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PedidoViewModel modelo)
    {
        var elegidas = modelo.Lineas.Where(l => l.Cantidad > 0).ToList();

        if (elegidas.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Elegí al menos una pizza.");
        }

        // Los precios se leen de la base, nunca de lo que mande el navegador
        var ids = elegidas.Select(l => l.PizzaId).Distinct().ToList();
        var pizzas = await _contexto.Pizzas.Where(p => ids.Contains(p.Id)).ToListAsync();

        if (pizzas.Count != ids.Count)
        {
            ModelState.AddModelError(string.Empty, "Alguna de las pizzas elegidas ya no existe.");
        }

        if (!ModelState.IsValid)
        {
            // Volvemos a mostrar el formulario con los datos cargados y los errores
            await CompletarLineasAsync(modelo);
            return View(modelo);
        }

        // Cliente: si el email ya existe se reutiliza
        var cliente = await _contexto.Clientes.FirstOrDefaultAsync(c => c.Email == modelo.Email);
        if (cliente == null)
        {
            cliente = new Cliente { Email = modelo.Email };
            _contexto.Clientes.Add(cliente);
        }
        cliente.Nombre = modelo.Nombre;
        cliente.Telefono = modelo.Telefono;
        cliente.Direccion = modelo.Direccion;

        var pedido = new Pedido { Cliente = cliente };

        foreach (var linea in elegidas)
        {
            var pizza = pizzas.First(p => p.Id == linea.PizzaId);
            pedido.Items.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                Cantidad = linea.Cantidad,
                PrecioUnitario = pizza.Precio,
            });
            pedido.Total += pizza.Precio * linea.Cantidad;
        }

        _contexto.Pedidos.Add(pedido);
        await _contexto.SaveChangesAsync();

        // TempData sobrevive a la redireccion (patron Post/Redirect/Get)
        TempData["Mensaje"] = $"¡Pedido #{pedido.Id} confirmado!";
        return RedirectToAction(nameof(Detalles), new { id = pedido.Id });
    }

    // Vuelve a cargar nombre/precio de cada linea (solo se postean PizzaId y Cantidad)
    private async Task CompletarLineasAsync(PedidoViewModel modelo)
    {
        var pizzas = await _contexto.Pizzas.ToListAsync();

        foreach (var linea in modelo.Lineas)
        {
            var pizza = pizzas.FirstOrDefault(p => p.Id == linea.PizzaId);
            if (pizza == null) continue;

            linea.Nombre = pizza.Nombre;
            linea.Descripcion = pizza.Descripcion;
            linea.Precio = pizza.Precio;
        }
    }
}
