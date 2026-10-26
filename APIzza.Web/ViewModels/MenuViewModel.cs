using APIzza.Web.Models;

namespace APIzza.Web.ViewModels;

/// <summary>
/// Lo que le pasa PizzasController.Index a la vista del menu:
/// la lista de pizzas + las categorias para los filtros + el filtro actual.
/// </summary>
public class MenuViewModel
{
    public List<Pizza> Pizzas { get; set; } = new();
    public List<string> Categorias { get; set; } = new();
    public string? CategoriaActual { get; set; }
}
