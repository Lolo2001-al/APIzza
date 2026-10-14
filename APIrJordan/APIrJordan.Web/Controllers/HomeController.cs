using APIrJordan.Web.Data;
using APIrJordan.Web.Models;
using APIrJordan.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Portada y página de error.</summary>
public class HomeController : Controller
{
    private readonly ApirJordanDbContext _contexto;

    public HomeController(ApirJordanDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /  -> asincrónico: el hilo no queda bloqueado mientras MySQL responde
    public async Task<IActionResult> Index()
    {
        var publicadas = _contexto.Zapatillas.Where(z => z.Activa);

        var modelo = new InicioViewModel
        {
            Destacadas = await publicadas
                .Where(z => z.Destacada)
                .Include(z => z.Talles)
                .OrderBy(z => z.Id)
                .Take(4)
                .ToListAsync(),

            Novedades = await publicadas
                .Include(z => z.Talles)
                .OrderByDescending(z => z.CreadoEn)
                .ThenByDescending(z => z.Id)
                .Take(4)
                .ToListAsync(),

            TotalModelos = await publicadas.CountAsync(),
            TotalPares = await _contexto.StockTalles.Where(s => s.Zapatilla!.Activa).SumAsync(s => s.Cantidad),
            PedidosEntregados = await _contexto.Pedidos.CountAsync(p => p.Estado == EstadoPedido.Entregado),
        };

        var conteo = await publicadas
            .GroupBy(z => z.Categoria)
            .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        modelo.ConteoPorCategoria = conteo.ToDictionary(c => c.Categoria, c => c.Cantidad);

        return View(modelo);
    }

    // GET /Home/Error?codigo=404  (la llama el middleware de páginas de estado y el de excepciones)
    public IActionResult Error(int? codigo)
    {
        var estado = codigo is >= 400 and <= 599 ? codigo.Value : 500;
        Response.StatusCode = estado;

        var modelo = estado switch
        {
            404 => new ErrorViewModel { Codigo = 404, Titulo = "Esta página no existe.", Mensaje = "Puede que el modelo o el pedido se hayan borrado, o que la dirección esté mal escrita." },
            400 => new ErrorViewModel { Codigo = 400, Titulo = "Pedido inválido.", Mensaje = "El navegador mandó algo que no pudimos interpretar. Volvé a intentar desde el inicio." },
            _ => new ErrorViewModel { Codigo = estado, Titulo = "Algo salió mal.", Mensaje = "Ocurrió un error inesperado. Si se repite, revisá que MySQL esté iniciado y la cadena de conexión sea correcta." },
        };

        return View(modelo);
    }
}
