using System.Globalization;

namespace APIrJordan.Web.Helpers;

/// <summary>
/// Formatos para mostrar. La aplicación corre con cultura invariante (los formularios mandan "40.5" y "389999")
/// y acá se formatea explícitamente a la manera argentina: $ 389.999 y 14/10/2026.
/// </summary>
public static class Formato
{
    private static readonly CultureInfo Argentina = CultureInfo.GetCultureInfo("es-AR");

    public static string Pesos(decimal valor) => "$ " + valor.ToString("N0", Argentina);

    public static string Fecha(DateTime fecha) => fecha.ToString("dd/MM/yyyy", Argentina);

    public static string FechaHora(DateTime fecha) => fecha.ToString("dd/MM/yyyy HH:mm", Argentina);

    /// <summary>"hace 3 días", "hace 2 horas"... para listados.</summary>
    public static string Relativo(DateTime fecha)
    {
        var diferencia = DateTime.Now - fecha;

        if (diferencia.TotalMinutes < 1) return "recién";
        if (diferencia.TotalHours < 1) return $"hace {(int)diferencia.TotalMinutes} min";
        if (diferencia.TotalDays < 1) return $"hace {(int)diferencia.TotalHours} h";
        if (diferencia.TotalDays < 30) return $"hace {(int)diferencia.TotalDays} d";

        return Fecha(fecha);
    }

    /// <summary>"1 par" / "3 pares".</summary>
    public static string Pares(int cantidad) => cantidad == 1 ? "1 par" : $"{cantidad} pares";
}
