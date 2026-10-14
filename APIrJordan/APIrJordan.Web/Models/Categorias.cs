namespace APIrJordan.Web.Models;

/// <summary>Las tres secciones del catálogo. Se usan como texto en la base (igual que las categorías de APIzza).</summary>
public static class Categorias
{
    public const string Retro = "retro";
    public const string Lifestyle = "lifestyle";
    public const string Basquet = "basquet";

    public static readonly string[] Todas = { Retro, Lifestyle, Basquet };

    public static bool EsValida(string? categoria) => categoria != null && Todas.Contains(categoria);

    public static string Nombre(string categoria) => categoria switch
    {
        Retro => "Retro",
        Lifestyle => "Lifestyle",
        Basquet => "Básquet",
        _ => categoria,
    };

    public static string Descripcion(string categoria) => categoria switch
    {
        Retro => "Las siluetas históricas, reeditadas tal como salieron.",
        Lifestyle => "Para todos los días: cómodas, limpias, combinables.",
        Basquet => "Pensadas para jugar: livianas y con amortiguación.",
        _ => string.Empty,
    };
}
