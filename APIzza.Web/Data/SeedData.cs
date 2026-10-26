using APIzza.Web.Models;

namespace APIzza.Web.Data;

/// <summary>Garantiza que la base exista y agrega pizzas de ejemplo solo si el menú está vacío.</summary>
public static class SeedData
{
    public static void Inicializar(ApizzaDbContext contexto)
    {
        contexto.Database.EnsureCreated();

        if (contexto.Pizzas.Any())
        {
            return;
        }

        contexto.Pizzas.AddRange(
            new Pizza { Nombre = "Muzzarella", Descripcion = "La clásica de siempre: salsa de tomate y muzzarella extra", Precio = 6500, Categoria = "clasica" },
            new Pizza { Nombre = "Napolitana", Descripcion = "Muzzarella, tomate en rodajas, ajo y aceite de oliva", Precio = 7200, Categoria = "clasica" },
            new Pizza { Nombre = "Fugazzeta", Descripcion = "Doble muzzarella y cebolla a la parrilla", Precio = 7500, Categoria = "especial" },
            new Pizza { Nombre = "Cuatro Quesos", Descripcion = "Muzzarella, provolone, roquefort y parmesano", Precio = 8300, Categoria = "especial" },
            new Pizza { Nombre = "Vegetariana", Descripcion = "Muzzarella, morrón, cebolla, aceitunas y choclo", Precio = 7800, Categoria = "vegetariana" });

        contexto.SaveChanges();
    }
}
