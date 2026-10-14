using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Lo que HomeController.Index le pasa a la portada.</summary>
public class InicioViewModel
{
    public List<Zapatilla> Destacadas { get; set; } = new();
    public List<Zapatilla> Novedades { get; set; } = new();
    public int TotalModelos { get; set; }
    public int TotalPares { get; set; }
    public int PedidosEntregados { get; set; }

    /// <summary>Modelos publicados por categoría (para las tarjetas de la portada).</summary>
    public Dictionary<string, int> ConteoPorCategoria { get; set; } = new();

    public int Conteo(string categoria) => ConteoPorCategoria.TryGetValue(categoria, out var cantidad) ? cantidad : 0;
}
