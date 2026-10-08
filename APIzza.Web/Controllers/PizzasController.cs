using APIzza.Web.Data;
using APIzza.Web.Models;
using APIzza.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIzza.Web.Controllers;

/// <summary>Menu de pizzas y alta de nuevas pizzas.</summary>
public class PizzasController : Controller
{
    private readonly ApizzaDbContext _contexto;

    public PizzasController(ApizzaDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /Pizzas?categoria=clasica
    public async Task<IActionResult> Index(string? categoria)
    {
        var consulta = _contexto.Pizzas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            consulta = consulta.Where(p => p.Categoria == categoria);
        }

        var modelo = new MenuViewModel
        {
            CategoriaActual = categoria,
            Categorias = await _contexto.Pizzas
                .Select(p => p.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync(),
            Pizzas = await consulta.OrderBy(p => p.Id).ToListAsync(),
        };

        return View(modelo);
    }

    // GET /Pizzas/Crear
    public IActionResult Crear(string? categoria)
    {
        var categoriasValidas = new[] { "clasica", "especial", "vegetariana" };
        var categoriaInicial = categoriasValidas.Contains(categoria ?? string.Empty)
            ? categoria!
            : "clasica";

        return View(new PizzaCrearViewModel { Categoria = categoriaInicial });
    }

    // POST /Pizzas/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PizzaCrearViewModel modelo)
    {
        var categoriasValidas = new[] { "clasica", "especial", "vegetariana" };

        if (!categoriasValidas.Contains(modelo.Categoria))
        {
            ModelState.AddModelError(nameof(modelo.Categoria), "Elegí una categoría válida.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var pizza = new Pizza
        {
            Nombre = modelo.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim(),
            Precio = modelo.Precio,
            Categoria = modelo.Categoria
        };

        _contexto.Pizzas.Add(pizza);
        await _contexto.SaveChangesAsync();

        TempData["Mensaje"] = $"La pizza '{pizza.Nombre}' fue agregada al menú.";
        return RedirectToAction(nameof(Index), new { categoria = pizza.Categoria });
    }

    // GET /Pizzas/Detalles/3
    public async Task<IActionResult> Detalles(int id)
    {
        Pizza? pizza = await _contexto.Pizzas.FindAsync(id);

        if (pizza == null)
        {
            return NotFound();
        }

        return View(pizza);
    }
}
