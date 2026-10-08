using APIzza.Web.Data;
using APIzza.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APIzza.Web.Controllers;

/// <summary>Pagina de inicio.</summary>
public class HomeController : Controller
{
    private readonly ApizzaDbContext _contexto;

    public HomeController(ApizzaDbContext contexto)
    {
        _contexto = contexto;
    }

    // GET /  -> asincrono: no bloquea el hilo mientras la base responde
    public async Task<IActionResult> Index()
    {
        // MODEL: las primeras 3 pizzas del menu
        List<Pizza> destacadas = await _contexto.Pizzas
            .OrderBy(p => p.Id)
            .Take(3)
            .ToListAsync();

        // El controlador le pasa el Model a la vista Views/Home/Index.cshtml
        return View(destacadas);
    }
}
