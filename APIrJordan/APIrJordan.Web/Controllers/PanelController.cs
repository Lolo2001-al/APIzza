using APIrJordan.Web.Data;
using APIrJordan.Web.Models;
using APIrJordan.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Panel con los indicadores del negocio (equivale a las consultas de reporte de database/consultas.sql).</summary>
public class PanelController : Controller
{
    private readonly ApirJordanDbContext _contexto;

    public PanelController(ApirJordanDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /Panel
    public async Task<IActionResult> Index()
    {
        var pedidos = await _contexto.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Items)
                .ThenInclude(i => i.Zapatilla)
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        // Los cancelados no cuentan como venta
        var ventas = pedidos.Where(p => !p.Cancelado).ToList();
        var items = ventas.SelectMany(p => p.Items).ToList();

        var modelo = new PanelViewModel
        {
            VentasTotales = ventas.Sum(p => p.Total),
            PedidosTotales = pedidos.Count,
            PedidosActivos = pedidos.Count(p => EstadoPedido.EsActivo(p.Estado)),
            ParesVendidos = items.Sum(i => i.Cantidad),
            TicketPromedio = ventas.Count == 0 ? 0 : ventas.Sum(p => p.Total) / ventas.Count,
            PedidosPorEstado = EstadoPedido.Todos.ToDictionary(e => e, e => pedidos.Count(p => p.Estado == e)),
            UltimosPedidos = pedidos.Take(5).ToList(),

            ModelosPublicados = await _contexto.Zapatillas.CountAsync(z => z.Activa),
            ModelosAgotados = await _contexto.Zapatillas.CountAsync(z => z.Activa && !z.Talles.Any(t => t.Cantidad > 0)),
            ParesEnStock = await _contexto.StockTalles.Where(s => s.Zapatilla!.Activa).SumAsync(s => s.Cantidad),

            // Talles con 1 o 2 pares: hay que reponer
            StockBajo = await _contexto.StockTalles
                .Include(s => s.Zapatilla)
                .Where(s => s.Zapatilla!.Activa && s.Cantidad > 0 && s.Cantidad <= 2)
                .OrderBy(s => s.Cantidad)
                .ThenBy(s => s.Zapatilla!.Nombre)
                .ThenBy(s => s.Talle)
                .Take(8)
                .ToListAsync(),
        };

        // Rankings (GROUP BY en memoria sobre los ítems vendidos)
        modelo.MasVendidas = ConBarras(items
            .GroupBy(i => i.ZapatillaId)
            .Select(g => new RankingViewModel
            {
                Etiqueta = g.First().Zapatilla?.NombreCompleto ?? $"Modelo #{g.Key}",
                Detalle = g.First().Zapatilla == null ? null : Categorias.Nombre(g.First().Zapatilla!.Categoria),
                Cantidad = g.Sum(i => i.Cantidad),
                Monto = g.Sum(i => i.Subtotal),
            })
            .OrderByDescending(r => r.Cantidad)
            .ThenByDescending(r => r.Monto)
            .Take(5));

        modelo.TallesMasVendidos = ConBarras(items
            .GroupBy(i => i.Talle)
            .Select(g => new RankingViewModel
            {
                Etiqueta = "Talle " + Talles.Formatear(g.Key),
                Cantidad = g.Sum(i => i.Cantidad),
                Monto = g.Sum(i => i.Subtotal),
            })
            .OrderByDescending(r => r.Cantidad)
            .ThenBy(r => r.Etiqueta)
            .Take(5));

        modelo.VentasPorCategoria = ConBarras(items
            .GroupBy(i => i.Zapatilla?.Categoria ?? "otra")
            .Select(g => new RankingViewModel
            {
                Etiqueta = Categorias.Nombre(g.Key),
                Cantidad = g.Sum(i => i.Cantidad),
                Monto = g.Sum(i => i.Subtotal),
            })
            .OrderByDescending(r => r.Monto), porMonto: true);

        return View(modelo);
    }

    /// <summary>Calcula el ancho de la barra de cada fila en relación con la primera (100 %), por cantidad o por monto.</summary>
    private static List<RankingViewModel> ConBarras(IEnumerable<RankingViewModel> filas, bool porMonto = false)
    {
        var lista = filas.ToList();
        var maximo = lista.Count == 0 ? 0 : lista.Max(r => porMonto ? r.Monto : r.Cantidad);

        foreach (var fila in lista)
        {
            var valor = porMonto ? fila.Monto : fila.Cantidad;
            fila.Porcentaje = maximo == 0 ? 0 : Math.Round((double)(valor * 100 / maximo), 1);
        }

        return lista;
    }
}
