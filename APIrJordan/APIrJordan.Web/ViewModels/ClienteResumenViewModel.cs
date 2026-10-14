using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Una fila del listado de clientes: datos + resumen de sus compras.</summary>
public class ClienteResumenViewModel
{
    public Cliente Cliente { get; set; } = new();
    public int CantidadPedidos { get; set; }
    public decimal TotalGastado { get; set; }
    public DateTime? UltimoPedido { get; set; }
}

public class ClientesIndexViewModel
{
    public List<ClienteResumenViewModel> Clientes { get; set; } = new();
    public string? Busqueda { get; set; }
}
