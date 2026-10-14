using APIrJordan.Web.Data;
using APIrJordan.Web.Models;
using APIrJordan.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Clientes y su historial de compras.</summary>
public class ClientesController : Controller
{
    private readonly ApirJordanDbContext _contexto;

    public ClientesController(ApirJordanDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /Clientes?q=rios
    public async Task<IActionResult> Index(string? q)
    {
        IQueryable<Cliente> consulta = _contexto.Clientes.Include(c => c.Pedidos);

        var busqueda = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        if (busqueda != null)
        {
            consulta = consulta.Where(c => c.Nombre.Contains(busqueda) || c.Email.Contains(busqueda));
        }

        var clientes = await consulta.OrderBy(c => c.Nombre).ToListAsync();

        // El resumen se calcula en memoria: son pocos clientes y así el código queda simple de leer
        var modelo = new ClientesIndexViewModel
        {
            Busqueda = busqueda,
            Clientes = clientes
                .Select(c =>
                {
                    var validos = c.Pedidos.Where(p => !p.Cancelado).ToList();
                    return new ClienteResumenViewModel
                    {
                        Cliente = c,
                        CantidadPedidos = validos.Count,
                        TotalGastado = validos.Sum(p => p.Total),
                        UltimoPedido = c.Pedidos.Count == 0 ? null : c.Pedidos.Max(p => p.CreadoEn),
                    };
                })
                .OrderByDescending(r => r.TotalGastado)
                .ThenBy(r => r.Cliente.Nombre)
                .ToList(),
        };

        return View(modelo);
    }

    // GET /Clientes/Detalles/2
    public async Task<IActionResult> Detalles(int id)
    {
        var cliente = await _contexto.Clientes
            .Include(c => c.Pedidos)
                .ThenInclude(p => p.Items)
                    .ThenInclude(i => i.Zapatilla)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null)
        {
            return NotFound();
        }

        return View(cliente);
    }
}
