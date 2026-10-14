using APIrJordan.Web.Data;
using APIrJordan.Web.Helpers;
using APIrJordan.Web.Models;
using APIrJordan.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Carrito de compras (vive en la sesión; los precios y el stock se leen siempre de la base).</summary>
public class CarritoController : Controller
{
    private readonly ApirJordanDbContext _contexto;
    private readonly CarritoService _carrito;

    public CarritoController(ApirJordanDbContext contexto, CarritoService carrito)
    {
        _contexto = contexto;
        _carrito = carrito;
    }

    // GET /Carrito
    public async Task<IActionResult> Index()
    {
        var modelo = await _carrito.ArmarResumenAsync();
        return View(modelo);
    }

    // POST /Carrito/Agregar  (desde la ficha del modelo)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int zapatillaId, decimal? talle, int cantidad = 1)
    {
        if (talle == null)
        {
            TempData["Error"] = "Elegí un talle antes de agregar al carrito.";
            return RedirectToAction("Detalles", "Zapatillas", new { id = zapatillaId });
        }

        var stock = await _contexto.StockTalles
            .Include(s => s.Zapatilla)
            .FirstOrDefaultAsync(s => s.ZapatillaId == zapatillaId && s.Talle == talle.Value);

        if (stock?.Zapatilla == null || !stock.Zapatilla.Activa)
        {
            TempData["Error"] = "Ese talle no está disponible para este modelo.";
            return RedirectToAction("Detalles", "Zapatillas", new { id = zapatillaId });
        }

        cantidad = Math.Clamp(cantidad, 1, CarritoService.MaximoPorLinea);
        var yaEnCarrito = _carrito.CantidadDe(zapatillaId, talle.Value);

        // El stock se controla acá y otra vez al confirmar la compra
        if (stock.Cantidad < yaEnCarrito + cantidad)
        {
            TempData["Error"] = stock.Cantidad == 0
                ? $"El talle {stock.TalleTexto} se agotó."
                : $"Del talle {stock.TalleTexto} quedan {Formato.Pares(stock.Cantidad)} y ya tenés {yaEnCarrito} en el carrito.";
            return RedirectToAction("Detalles", "Zapatillas", new { id = zapatillaId, talle = talle.Value });
        }

        _carrito.Agregar(zapatillaId, talle.Value, cantidad);

        TempData["Mensaje"] = $"Agregaste {stock.Zapatilla.NombreCompleto} (talle {stock.TalleTexto}) al carrito.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Carrito/Actualizar  (cambiar la cantidad de una línea)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Actualizar(int zapatillaId, decimal talle, int cantidad)
    {
        cantidad = Math.Clamp(cantidad, 0, CarritoService.MaximoPorLinea);

        if (cantidad > 0)
        {
            var stock = await _contexto.StockTalles
                .FirstOrDefaultAsync(s => s.ZapatillaId == zapatillaId && s.Talle == talle);

            var disponible = stock?.Cantidad ?? 0;

            if (disponible < cantidad)
            {
                cantidad = disponible;
                TempData["Error"] = disponible == 0
                    ? "Ese talle se agotó y se quitó del carrito."
                    : $"Solo quedan {Formato.Pares(disponible)} de ese talle: ajustamos la cantidad.";
            }
        }

        _carrito.Actualizar(zapatillaId, talle, cantidad);
        return RedirectToAction(nameof(Index));
    }

    // POST /Carrito/Quitar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Quitar(int zapatillaId, decimal talle)
    {
        _carrito.Quitar(zapatillaId, talle);
        TempData["Mensaje"] = "Quitaste el par del carrito.";
        return RedirectToAction(nameof(Index));
    }

    // POST /Carrito/Vaciar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Vaciar()
    {
        _carrito.Vaciar();
        TempData["Mensaje"] = "El carrito quedó vacío.";
        return RedirectToAction("Index", "Zapatillas");
    }
}
