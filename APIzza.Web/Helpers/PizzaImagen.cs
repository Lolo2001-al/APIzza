namespace APIzza.Web.Helpers;

/// <summary>Elige una ilustración local para las pizzas del menú de ejemplo.</summary>
public static class PizzaImagen
{
    public static string Obtener(string nombre) => nombre.Trim().ToLowerInvariant() switch
    {
        "napolitana" => "~/images/pizzas/napolitana.svg",
        "fugazzeta" => "~/images/pizzas/fugazzeta.svg",
        "cuatro quesos" => "~/images/pizzas/cuatro-quesos.svg",
        "vegetariana" => "~/images/pizzas/vegetariana.svg",
        _ => "~/images/pizzas/muzzarella.svg"
    };
}
