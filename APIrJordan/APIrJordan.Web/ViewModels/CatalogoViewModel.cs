using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>
/// Lo que ZapatillasController.Index le pasa al catálogo:
/// la página de resultados + los filtros aplicados + los datos para dibujar los filtros.
/// </summary>
public class CatalogoViewModel
{
    public List<Zapatilla> Zapatillas { get; set; } = new();

    // Filtros aplicados (ya validados por el controlador)
    public string? Categoria { get; set; }
    public decimal? Talle { get; set; }
    public string? Busqueda { get; set; }
    public string Orden { get; set; } = "nuevos";

    // Paginación
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalResultados { get; set; }

    /// <summary>Cantidad de modelos publicados por categoría (para los contadores de las pestañas).</summary>
    public Dictionary<string, int> ConteoPorCategoria { get; set; } = new();

    public bool HayFiltros => Categoria != null || Talle != null || !string.IsNullOrWhiteSpace(Busqueda);

    public static readonly (string Valor, string Texto)[] Ordenes =
    {
        ("nuevos", "Novedades"),
        ("precio_asc", "Precio: menor a mayor"),
        ("precio_desc", "Precio: mayor a menor"),
        ("nombre", "Nombre A-Z"),
    };
}
