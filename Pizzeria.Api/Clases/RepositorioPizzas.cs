namespace Pizzeria.Api.Clases;

/// <summary>Guarda la carta de pizzas. Hoy es una lista fija en memoria, pero cualquier
/// controlador la usa de forma asincrónica, lista para el día que esto sea una base de datos real.</summary>
public class RepositorioPizzas
{
    private readonly List<Pizza> _pizzas = new()
    {
        new(1, "Muzzarella", "Salsa de tomate, muzzarella y aceitunas", 9500),
        new(2, "Napolitana", "Muzzarella, rodajas de tomate, ajo y albahaca", 11000),
        new(3, "Fugazzeta", "Rellena de muzzarella con cebolla caramelizada", 12500),
        new(4, "Calabresa", "Muzzarella y longaniza calabresa picante", 12000),
        new(5, "Cuatro Quesos", "Muzzarella, provolone, roquefort y parmesano", 13500),
        new(6, "Jamón y Morrones", "Muzzarella, jamón cocido y morrones asados", 12000),
    };

    public Task<List<Pizza>> ObtenerTodasAsync() => Task.FromResult(_pizzas);

    public Task<Pizza?> ObtenerPorIdAsync(int id) =>
        Task.FromResult(_pizzas.FirstOrDefault(p => p.Id == id));
}
