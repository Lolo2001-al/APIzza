using APIzza.Web.Data;
using APIzza.Web.Models;
using APIzza.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIzza.Web.Controllers;

/// <summary>Menu de pizzas (solo lectura).</summary>
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

        // Armamos un ViewModel: todo lo que la vista necesita en un solo objeto
        var modelo = new MenuViewModel
        {
            CategoriaActual = categoria,
            Categorias = await _contexto.Pizzas
                .Select(p => p.Categoria)
                .Distinct()
                .ToListAsync(),
            Pizzas = await consulta.OrderBy(p => p.Id).ToListAsync(),
        };

        return View(modelo);
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
