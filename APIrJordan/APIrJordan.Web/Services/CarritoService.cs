using System.Text.Json;
using APIrJordan.Web.Data;
using APIrJordan.Web.Helpers;
using APIrJordan.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Services;

/// <summary>Una línea del carrito: qué modelo, en qué talle y cuántos pares. Vive en la sesión del navegador.</summary>
public class ItemCarrito
{
    public int ZapatillaId { get; set; }
    public decimal Talle { get; set; }
    public int Cantidad { get; set; }
}

/// <summary>
/// Carrito de compras guardado en la sesión (serializado como JSON).
/// El carrito solo guarda ids, talles y cantidades: los nombres, precios y el stock se leen siempre de la base.
/// </summary>
public class CarritoService
{
    public const int MaximoPorLinea = 5;
    private const string Clave = "carrito";

    private readonly IHttpContextAccessor _accessor;
    private readonly ApirJordanDbContext _contexto;

    public CarritoService(IHttpContextAccessor accessor, ApirJordanDbContext contexto)
    {
        _accessor = accessor;
        _contexto = contexto;
    }

    private ISession Sesion => _accessor.HttpContext?.Session
        ?? throw new InvalidOperationException("No hay una sesión HTTP disponible.");

    public List<ItemCarrito> Obtener()
    {
        var json = Sesion.GetString(Clave);

        if (string.IsNullOrEmpty(json))
        {
            return new List<ItemCarrito>();
        }

        return JsonSerializer.Deserialize<List<ItemCarrito>>(json) ?? new List<ItemCarrito>();
    }

    private void Guardar(List<ItemCarrito> items)
    {
        if (items.Count == 0)
        {
            Sesion.Remove(Clave);
            return;
        }

        Sesion.SetString(Clave, JsonSerializer.Serialize(items));
    }

    /// <summary>Total de pares en el carrito (para el contador de la barra).</summary>
    public int CantidadTotal() => Obtener().Sum(i => i.Cantidad);

    public int CantidadDe(int zapatillaId, decimal talle) =>
        Obtener().FirstOrDefault(i => i.ZapatillaId == zapatillaId && i.Talle == talle)?.Cantidad ?? 0;

    public void Agregar(int zapatillaId, decimal talle, int cantidad)
    {
        var items = Obtener();
        var existente = items.FirstOrDefault(i => i.ZapatillaId == zapatillaId && i.Talle == talle);

        if (existente == null)
        {
            items.Add(new ItemCarrito { ZapatillaId = zapatillaId, Talle = talle, Cantidad = cantidad });
        }
        else
        {
            existente.Cantidad = Math.Min(MaximoPorLinea, existente.Cantidad + cantidad);
        }

        Guardar(items);
    }

    /// <summary>Fija la cantidad de una línea. Con 0 la elimina.</summary>
    public void Actualizar(int zapatillaId, decimal talle, int cantidad)
    {
        var items = Obtener();
        var existente = items.FirstOrDefault(i => i.ZapatillaId == zapatillaId && i.Talle == talle);

        if (existente == null)
        {
            return;
        }

        if (cantidad <= 0)
        {
            items.Remove(existente);
        }
        else
        {
            existente.Cantidad = Math.Min(MaximoPorLinea, cantidad);
        }

        Guardar(items);
    }

    public void Quitar(int zapatillaId, decimal talle)
    {
        var items = Obtener();
        items.RemoveAll(i => i.ZapatillaId == zapatillaId && i.Talle == talle);
        Guardar(items);
    }

    public void Vaciar() => Sesion.Remove(Clave);

    /// <summary>
    /// Convierte el carrito (ids) en líneas con datos reales de la base: nombre, precio y stock actual.
    /// Marca con un problema las líneas que ya no se pueden comprar tal como están.
    /// Las zapatillas quedan rastreadas por el DbContext: el checkout descuenta el stock sobre estas mismas entidades.
    /// </summary>
    public async Task<CarritoViewModel> ArmarResumenAsync()
    {
        var items = Obtener();
        var modelo = new CarritoViewModel();

        if (items.Count == 0)
        {
            return modelo;
        }

        var ids = items.Select(i => i.ZapatillaId).Distinct().ToList();
        var zapatillas = await _contexto.Zapatillas
            .Include(z => z.Talles)
            .Where(z => ids.Contains(z.Id))
            .ToListAsync();

        foreach (var item in items)
        {
            var zapatilla = zapatillas.FirstOrDefault(z => z.Id == item.ZapatillaId);

            if (zapatilla == null)
            {
                continue;   // el modelo se borró: la línea desaparece
            }

            var stock = zapatilla.Talles.FirstOrDefault(t => t.Talle == item.Talle);
            var disponible = stock?.Cantidad ?? 0;

            var linea = new LineaCarritoViewModel
            {
                Zapatilla = zapatilla,
                Talle = item.Talle,
                Cantidad = item.Cantidad,
                StockDisponible = disponible,
            };

            if (!zapatilla.Activa)
            {
                linea.Problema = "Este modelo ya no está publicado.";
            }
            else if (disponible == 0)
            {
                linea.Problema = "Se agotó este talle.";
            }
            else if (disponible < item.Cantidad)
            {
                linea.Problema = $"Solo quedan {Formato.Pares(disponible)} de este talle.";
            }

            modelo.Lineas.Add(linea);
        }

        return modelo;
    }
}
