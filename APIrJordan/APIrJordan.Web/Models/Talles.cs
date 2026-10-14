using System.Globalization;

namespace APIrJordan.Web.Models;

/// <summary>Curva de talles que maneja la tienda.</summary>
public static class Talles
{
    public static readonly decimal[] Todos = { 38m, 39m, 40m, 40.5m, 41m, 42m, 42.5m, 43m, 44m, 45m };

    public static bool EsValido(decimal talle) => Todos.Contains(talle);

    /// <summary>40.5 → "40.5" y 41 → "41". Siempre con punto, para que sirva como valor de formulario y de URL.</summary>
    public static string Formatear(decimal talle) => talle.ToString("0.#", CultureInfo.InvariantCulture);
}
