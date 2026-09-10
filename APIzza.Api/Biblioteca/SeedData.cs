using APIzza.Api.ClasesPizzas;

namespace APIzza.Api.Biblioteca;

/// <summary>
/// Se encarga de crear la base de datos (si no existe) y cargarla
/// con un menu de pizzas de ejemplo la primera vez que se levanta el server.
/// </summary>
public static class SeedData
{
    public static void Inicializar(ApizzaDbContext contexto)
    {
        contexto.Database.EnsureCreated();

        if (contexto.Pizzas.Any())
        {
            return; // ya hay datos cargados, no se vuelve a sembrar
        }

        var pizzas = new List<Pizza>
        {
            new Pizza
            {
                Nombre = "Muzzarella",
                Descripcion = "La clasica de siempre: salsa de tomate y muzzarella extra",
                Precio = 6500,
                Categoria = "clasica",
                Imagen = "muzzarella.jpg",
            },
            new Pizza
            {
                Nombre = "Napolitana",
                Descripcion = "Muzzarella, tomate en rodajas, ajo y aceite de oliva",
                Precio = 7200,
                Categoria = "clasica",
                Imagen = "napolitana.jpg",
            },
            new Pizza
            {
                Nombre = "Fugazzeta",
                Descripcion = "Doble muzzarella y cebolla a la parrilla",
                Precio = 7500,
                Categoria = "especial",
                Imagen = "fugazzeta.jpg",
            },
            new Pizza
            {
                Nombre = "Cuatro Quesos",
                Descripcion = "Muzzarella, provolone, roquefort y parmesano",
                Precio = 8300,
                Categoria = "especial",
                Imagen = "cuatro-quesos.jpg",
            },
            new Pizza
            {
                Nombre = "Vegetariana",
                Descripcion = "Muzzarella, morron, cebolla, aceitunas y choclo",
                Precio = 7800,
                Categoria = "vegetariana",
                Imagen = "vegetariana.jpg",
            },
        };

        contexto.Pizzas.AddRange(pizzas);
        contexto.SaveChanges();
    }
}
