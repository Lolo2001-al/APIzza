using APIrJordan.Web.Data;
using APIrJordan.Web.Helpers;
using APIrJordan.Web.Models;
using APIrJordan.Web.Services;
using APIrJordan.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Checkout (carrito → pedido), listado, detalle y cambios de estado de los pedidos.</summary>
public class PedidosController : Controller
{
    private readonly ApirJordanDbContext _contexto;
    private readonly CarritoService _carrito;

    public PedidosController(ApirJordanDbContext contexto, CarritoService carrito)
    {
        _contexto = contexto;
        _carrito = carrito;
    }

    // GET /Pedidos?estado=pendiente&q=valentina
    public async Task<IActionResult> Index(string? estado, string? q)
    {
        IQueryable<Pedido> consulta = _contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Items);

        if (EstadoPedido.EsValido(estado))
        {
            consulta = consulta.Where(p => p.Estado == estado);
        }
        else
        {
            estado = null;
        }

        var busqueda = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        if (busqueda != null)
        {
            if (int.TryParse(busqueda.TrimStart('#'), out var numero))
            {
                consulta = consulta.Where(p => p.Id == numero);
            }
            else
            {
                consulta = consulta.Where(p => p.Cliente!.Nombre.Contains(busqueda) || p.Cliente.Email.Contains(busqueda));
            }
        }

        var pedidos = await consulta
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        var conteo = await _contexto.Pedidos
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var modelo = new PedidosIndexViewModel
        {
            Pedidos = pedidos,
            Estado = estado,
            Busqueda = busqueda,
            ConteoPorEstado = conteo.ToDictionary(c => c.Estado, c => c.Cantidad),
        };

        return View(modelo);
    }

    // GET /Pedidos/Detalles/5
    public async Task<IActionResult> Detalles(int id)
    {
        var pedido = await BuscarCompletoAsync(id);

        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    // GET /Pedidos/Checkout  -> resumen del carrito + datos del comprador
    public async Task<IActionResult> Checkout()
    {
        var carrito = await _carrito.ArmarResumenAsync();

        if (carrito.EstaVacio)
        {
            TempData["Error"] = "Tu carrito está vacío. Elegí un modelo y un talle para empezar.";
            return RedirectToAction("Index", "Zapatillas");
        }

        if (carrito.TieneProblemas)
        {
            TempData["Error"] = "Hay líneas del carrito sin stock suficiente. Revisalas antes de continuar.";
            return RedirectToAction("Index", "Carrito");
        }

        var modelo = new CheckoutViewModel
        {
            Lineas = carrito.Lineas,
            Total = carrito.Total,
        };

        return View(modelo);
    }

    // POST /Pedidos/Checkout  -> crea Cliente (si hace falta), Pedido e Items y descuenta el stock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel modelo)
    {
        // Las líneas se vuelven a armar desde la sesión + la base: nunca se confía en lo que manda el navegador
        var carrito = await _carrito.ArmarResumenAsync();

        if (carrito.EstaVacio)
        {
            TempData["Error"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Zapatillas");
        }

        if (carrito.TieneProblemas)
        {
            ModelState.AddModelError(string.Empty, "Alguna línea ya no tiene stock suficiente. Revisá el carrito.");
        }

        if (!ModelState.IsValid)
        {
            modelo.Lineas = carrito.Lineas;
            modelo.Total = carrito.Total;
            return View(modelo);
        }

        // Cliente: si el email ya existe se reutiliza y se actualizan sus datos
        var email = modelo.Email.Trim().ToLowerInvariant();
        var cliente = await _contexto.Clientes.FirstOrDefaultAsync(c => c.Email == email);

        if (cliente == null)
        {
            cliente = new Cliente { Email = email };
            _contexto.Clientes.Add(cliente);
        }

        cliente.Nombre = modelo.Nombre.Trim();
        cliente.Telefono = Limpiar(modelo.Telefono);
        cliente.Direccion = modelo.Direccion.Trim();

        var pedido = new Pedido
        {
            Cliente = cliente,
            Nota = Limpiar(modelo.Nota),
            Estado = EstadoPedido.Pendiente,
        };

        foreach (var linea in carrito.Lineas)
        {
            // El stock está rastreado por el DbContext (lo cargó ArmarResumenAsync): el descuento viaja en el mismo SaveChanges
            var stock = linea.Zapatilla.Talles.First(t => t.Talle == linea.Talle);
            stock.Cantidad -= linea.Cantidad;

            pedido.Items.Add(new ItemPedido
            {
                ZapatillaId = linea.Zapatilla.Id,
                Talle = linea.Talle,
                Cantidad = linea.Cantidad,
                PrecioUnitario = linea.Zapatilla.Precio,     // precio de la base, no del formulario
            });
        }

        pedido.Total = pedido.Items.Sum(i => i.Subtotal);      // el total se calcula en el servidor
        _contexto.Pedidos.Add(pedido);

        try
        {
            // Cliente + Pedido + Items + descuento de stock: todo o nada (una sola transacción)
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Otra persona compró alguno de estos pares entre que se mostró el checkout y se confirmó
            TempData["Error"] = "Justo se vendió alguno de los pares que elegiste. Revisá las cantidades e intentá de nuevo.";
            return RedirectToAction("Index", "Carrito");
        }

        _carrito.Vaciar();

        // Patrón Post/Redirect/Get: si el usuario recarga, no se duplica el pedido
        return RedirectToAction(nameof(Confirmacion), new { id = pedido.Id });
    }

    // GET /Pedidos/Confirmacion/5
    public async Task<IActionResult> Confirmacion(int id)
    {
        var pedido = await BuscarCompletoAsync(id);

        if (pedido == null)
        {
            return NotFound();
        }

        return View(pedido);
    }

    // POST /Pedidos/CambiarEstado/5  (estado=pagado|enviado|entregado|cancelado)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string estado)
    {
        var pedido = await _contexto.Pedidos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        if (!EstadoPedido.EsValido(estado) || !EstadoPedido.PuedePasarA(pedido.Estado, estado))
        {
            TempData["Error"] = $"Un pedido {EstadoPedido.Nombre(pedido.Estado).ToLowerInvariant()} no puede pasar a \"{estado}\".";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        if (estado == EstadoPedido.Cancelado)
        {
            // Al cancelar, los pares vuelven al stock del talle correspondiente
            foreach (var item in pedido.Items)
            {
                var stock = await _contexto.StockTalles
                    .FirstOrDefaultAsync(s => s.ZapatillaId == item.ZapatillaId && s.Talle == item.Talle);

                if (stock == null)
                {
                    _contexto.StockTalles.Add(new StockTalle { ZapatillaId = item.ZapatillaId, Talle = item.Talle, Cantidad = item.Cantidad });
                }
                else
                {
                    stock.Cantidad += item.Cantidad;
                }
            }
        }

        pedido.Estado = estado;
        pedido.ActualizadoEn = DateTime.Now;

        try
        {
            // Cambio de estado + reposición de stock (si corresponde) en una sola transacción
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "El stock cambió justo en este momento (alguien compró un par). Intentá de nuevo.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        TempData["Mensaje"] = $"Pedido #{pedido.Id}: {EstadoPedido.Nombre(estado).ToLowerInvariant()}.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    private Task<Pedido?> BuscarCompletoAsync(int id) =>
        _contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Items)
                .ThenInclude(i => i.Zapatilla)
            .FirstOrDefaultAsync(p => p.Id == id);

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
