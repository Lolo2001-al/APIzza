using APIrJordan.Web.Data;
using APIrJordan.Web.Helpers;
using APIrJordan.Web.Models;
using APIrJordan.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIrJordan.Web.Controllers;

/// <summary>Catálogo, ficha, alta, edición y baja de modelos.</summary>
public class ZapatillasController : Controller
{
    private const int TamanoPagina = 8;

    private readonly ApirJordanDbContext _contexto;

    public ZapatillasController(ApirJordanDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /Zapatillas?categoria=retro&talle=42&q=jordan&orden=precio_asc&pagina=2
    public async Task<IActionResult> Index(string? categoria, decimal? talle, string? q, string? orden, int pagina = 1)
    {
        IQueryable<Zapatilla> consulta = _contexto.Zapatillas.Where(z => z.Activa);

        // --- Filtros (cada uno agrega un WHERE a la misma consulta) ---
        if (Categorias.EsValida(categoria))
        {
            consulta = consulta.Where(z => z.Categoria == categoria);
        }
        else
        {
            categoria = null;
        }

        if (talle.HasValue && Talles.EsValido(talle.Value))
        {
            var talleBuscado = talle.Value;
            consulta = consulta.Where(z => z.Talles.Any(t => t.Talle == talleBuscado && t.Cantidad > 0));
        }
        else
        {
            talle = null;
        }

        var busqueda = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        if (busqueda != null)
        {
            consulta = consulta.Where(z => z.Nombre.Contains(busqueda) || (z.Colorway != null && z.Colorway.Contains(busqueda)));
        }

        // --- Orden ---
        if (orden is not ("precio_asc" or "precio_desc" or "nombre"))
        {
            orden = "nuevos";
        }

        consulta = orden switch
        {
            "precio_asc" => consulta.OrderBy(z => z.Precio).ThenBy(z => z.Id),
            "precio_desc" => consulta.OrderByDescending(z => z.Precio).ThenBy(z => z.Id),
            "nombre" => consulta.OrderBy(z => z.Nombre).ThenBy(z => z.Id),
            _ => consulta.OrderByDescending(z => z.CreadoEn).ThenByDescending(z => z.Id),
        };

        // --- Paginación ---
        var totalResultados = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalResultados / (double)TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var zapatillas = await consulta
            .Include(z => z.Talles)
            .Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .ToListAsync();

        // Contadores por categoría (GROUP BY en SQL) para las pestañas del filtro
        var conteo = await _contexto.Zapatillas
            .Where(z => z.Activa)
            .GroupBy(z => z.Categoria)
            .Select(g => new { Categoria = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        var modelo = new CatalogoViewModel
        {
            Zapatillas = zapatillas,
            Categoria = categoria,
            Talle = talle,
            Busqueda = busqueda,
            Orden = orden,
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            TotalResultados = totalResultados,
            ConteoPorCategoria = conteo.ToDictionary(c => c.Categoria, c => c.Cantidad),
        };

        return View(modelo);
    }

    // GET /Zapatillas/Detalles/3?talle=42
    public async Task<IActionResult> Detalles(int id, decimal? talle)
    {
        var zapatilla = await _contexto.Zapatillas
            .Include(z => z.Talles)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zapatilla == null)
        {
            return NotFound();
        }

        var relacionadas = await _contexto.Zapatillas
            .Include(z => z.Talles)
            .Where(z => z.Activa && z.Id != id && z.Categoria == zapatilla.Categoria)
            .OrderBy(z => z.Id)
            .Take(3)
            .ToListAsync();

        var modelo = new ZapatillaDetalleViewModel
        {
            Zapatilla = zapatilla,
            Relacionadas = relacionadas,
            TalleSeleccionado = talle,
            SeVendio = await _contexto.ItemsPedido.AnyAsync(i => i.ZapatillaId == id),
        };

        return View(modelo);
    }

    // GET /Zapatillas/Crear
    public IActionResult Crear()
    {
        var modelo = new ZapatillaFormViewModel();
        CompletarStock(modelo);
        return View(modelo);
    }

    // POST /Zapatillas/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ZapatillaFormViewModel modelo)
    {
        ValidarFormulario(modelo);

        if (!ModelState.IsValid)
        {
            CompletarStock(modelo);
            return View(modelo);
        }

        var zapatilla = new Zapatilla();
        Aplicar(modelo, zapatilla);

        foreach (var talle in Talles.Todos)
        {
            var fila = modelo.Stock.FirstOrDefault(s => s.Talle == talle);
            zapatilla.Talles.Add(new StockTalle { Talle = talle, Cantidad = fila?.Cantidad ?? 0 });
        }

        _contexto.Zapatillas.Add(zapatilla);
        await _contexto.SaveChangesAsync();

        TempData["Mensaje"] = $"{zapatilla.NombreCompleto} ya está en el catálogo.";
        return RedirectToAction(nameof(Detalles), new { id = zapatilla.Id });
    }

    // GET /Zapatillas/Editar/3
    public async Task<IActionResult> Editar(int id)
    {
        var zapatilla = await _contexto.Zapatillas
            .Include(z => z.Talles)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zapatilla == null)
        {
            return NotFound();
        }

        var modelo = new ZapatillaFormViewModel
        {
            Id = zapatilla.Id,
            Nombre = zapatilla.Nombre,
            Colorway = zapatilla.Colorway,
            Descripcion = zapatilla.Descripcion,
            Precio = zapatilla.Precio,
            Categoria = zapatilla.Categoria,
            Imagen = zapatilla.Imagen,
            Destacada = zapatilla.Destacada,
            Activa = zapatilla.Activa,
            Stock = zapatilla.Talles
                .OrderBy(t => t.Talle)
                .Select(t => new StockTalleFormViewModel { Talle = t.Talle, Cantidad = t.Cantidad })
                .ToList(),
        };
        CompletarStock(modelo);

        return View(modelo);
    }

    // POST /Zapatillas/Editar/3
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, ZapatillaFormViewModel modelo)
    {
        modelo.Id = id;
        ValidarFormulario(modelo);

        if (!ModelState.IsValid)
        {
            CompletarStock(modelo);
            return View(modelo);
        }

        var zapatilla = await _contexto.Zapatillas
            .Include(z => z.Talles)
            .FirstOrDefaultAsync(z => z.Id == id);

        if (zapatilla == null)
        {
            return NotFound();
        }

        Aplicar(modelo, zapatilla);

        // Stock: se actualiza la fila de cada talle (o se crea si faltaba)
        foreach (var fila in modelo.Stock.Where(s => Talles.EsValido(s.Talle)))
        {
            var existente = zapatilla.Talles.FirstOrDefault(t => t.Talle == fila.Talle);

            if (existente == null)
            {
                zapatilla.Talles.Add(new StockTalle { Talle = fila.Talle, Cantidad = fila.Cantidad });
            }
            else
            {
                existente.Cantidad = fila.Cantidad;
            }
        }

        try
        {
            await _contexto.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Alguien compró mientras se editaba: el stock que se leyó ya no es el real.
            ModelState.AddModelError(string.Empty, "El stock cambió mientras editabas (se vendió algún par). Volvé a abrir el formulario y cargá los valores de nuevo.");
            CompletarStock(modelo);
            return View(modelo);
        }

        TempData["Mensaje"] = $"Se guardaron los cambios de {zapatilla.NombreCompleto}.";
        return RedirectToAction(nameof(Detalles), new { id });
    }

    // POST /Zapatillas/Pausar/3  -> alterna Activa (pausar / volver a publicar)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pausar(int id)
    {
        var zapatilla = await _contexto.Zapatillas.FindAsync(id);

        if (zapatilla == null)
        {
            return NotFound();
        }

        zapatilla.Activa = !zapatilla.Activa;
        await _contexto.SaveChangesAsync();

        TempData["Mensaje"] = zapatilla.Activa
            ? $"{zapatilla.NombreCompleto} vuelve a estar publicada."
            : $"{zapatilla.NombreCompleto} quedó pausada: no se muestra ni se puede comprar.";

        return RedirectToAction(nameof(Detalles), new { id });
    }

    // POST /Zapatillas/Eliminar/3
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var zapatilla = await _contexto.Zapatillas.FindAsync(id);

        if (zapatilla == null)
        {
            return NotFound();
        }

        // La FK ItemsPedido -> Zapatillas es RESTRICT: un modelo vendido no se puede borrar, solo pausar.
        var seVendio = await _contexto.ItemsPedido.AnyAsync(i => i.ZapatillaId == id);

        if (seVendio)
        {
            TempData["Error"] = "Este modelo ya tiene ventas registradas: no se puede eliminar. Podés pausarlo para que deje de mostrarse.";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        _contexto.Zapatillas.Remove(zapatilla);      // el stock se borra en cascada
        await _contexto.SaveChangesAsync();

        TempData["Mensaje"] = $"{zapatilla.NombreCompleto} se eliminó del catálogo.";
        return RedirectToAction(nameof(Index));
    }

    // ----- Métodos privados compartidos por Crear y Editar -----

    private void ValidarFormulario(ZapatillaFormViewModel modelo)
    {
        if (!Categorias.EsValida(modelo.Categoria))
        {
            ModelState.AddModelError(nameof(modelo.Categoria), "Elegí una categoría válida.");
        }

        if (!string.IsNullOrEmpty(modelo.Imagen) && !ZapatillaImagen.Existe(modelo.Imagen))
        {
            ModelState.AddModelError(nameof(modelo.Imagen), "Elegí una ilustración de la lista.");
        }

        if (modelo.Precio != Math.Floor(modelo.Precio))
        {
            ModelState.AddModelError(nameof(modelo.Precio), "Cargá el precio en pesos enteros, sin centavos.");
        }
    }

    /// <summary>Copia los campos del formulario al modelo (sin tocar el stock).</summary>
    private static void Aplicar(ZapatillaFormViewModel modelo, Zapatilla zapatilla)
    {
        zapatilla.Nombre = modelo.Nombre.Trim();
        zapatilla.Colorway = Limpiar(modelo.Colorway);
        zapatilla.Descripcion = Limpiar(modelo.Descripcion);
        zapatilla.Precio = modelo.Precio;
        zapatilla.Categoria = modelo.Categoria;
        zapatilla.Imagen = Limpiar(modelo.Imagen);
        zapatilla.Destacada = modelo.Destacada;
        zapatilla.Activa = modelo.Activa;
    }

    /// <summary>Garantiza que el formulario tenga una fila por cada talle de la curva, en orden.</summary>
    private static void CompletarStock(ZapatillaFormViewModel modelo)
    {
        var completo = Talles.Todos
            .Select(talle => modelo.Stock.FirstOrDefault(s => s.Talle == talle) ?? new StockTalleFormViewModel { Talle = talle })
            .ToList();

        modelo.Stock = completo;
    }

    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
