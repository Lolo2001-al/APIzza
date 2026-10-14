using APIrJordan.Web.Models;

namespace APIrJordan.Web.Helpers;

/// <summary>Resuelve qué ilustración mostrar para cada modelo.</summary>
public static class ZapatillaImagen
{
    private const string Carpeta = "~/images/zapatillas/";

    /// <summary>Se usa cuando el modelo no tiene una imagen asignada.</summary>
    public const string Generica = "~/images/zapatillas/generica.svg";

    /// <summary>Ilustraciones incluidas en wwwroot, para elegir en el formulario de alta/edición.</summary>
    public static readonly IReadOnlyList<(string Archivo, string Nombre)> Disponibles = new List<(string, string)>
    {
        ("aj1-high-chicago.jpg", "Air Jordan 1 High — Chicago"),
        ("aj1-low-bred-toe.jpg", "Air Jordan 1 Low — Bred Toe"),
        ("aj3-white-cement.jpg", "Air Jordan 3 — White Cement"),
        ("aj4-bred.jpg", "Air Jordan 4 — Bred"),
        ("aj11-concord.jpg", "Air Jordan 11 — Concord"),
        ("aj12-taxi.jpg", "Air Jordan 12 — Taxi"),
        ("spizike-low.jpg", "Jordan Spizike Low"),
        ("max-aura-6.jpg", "Jordan Max Aura 6"),
        ("luka-3.jpg", "Jordan Luka 3"),
        ("tatum-3.jpg", "Jordan Tatum 3"),
    };

    public static bool Existe(string? archivo) => archivo != null && Disponibles.Any(d => d.Archivo == archivo);

    public static string Url(Zapatilla zapatilla) => Url(zapatilla.Imagen);

    public static string Url(string? archivo) => Existe(archivo) ? Carpeta + archivo : Generica;
}
