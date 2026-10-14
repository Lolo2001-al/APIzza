using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Listado de pedidos con el filtro por estado y la búsqueda aplicados.</summary>
public class PedidosIndexViewModel
{
    public List<Pedido> Pedidos { get; set; } = new();
    public string? Estado { get; set; }
    public string? Busqueda { get; set; }

    /// <summary>Cantidad de pedidos por estado (para las pestañas).</summary>
    public Dictionary<string, int> ConteoPorEstado { get; set; } = new();

    public int Total => ConteoPorEstado.Values.Sum();

    public int Conteo(string estado) => ConteoPorEstado.TryGetValue(estado, out var cantidad) ? cantidad : 0;
}
