using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Indicadores del negocio para la pantalla "Panel".</summary>
public class PanelViewModel
{
    public decimal VentasTotales { get; set; }
    public int PedidosTotales { get; set; }
    public int PedidosActivos { get; set; }
    public int ParesVendidos { get; set; }
    public decimal TicketPromedio { get; set; }
    public int ModelosPublicados { get; set; }
    public int ModelosAgotados { get; set; }
    public int ParesEnStock { get; set; }

    public Dictionary<string, int> PedidosPorEstado { get; set; } = new();
    public List<RankingViewModel> MasVendidas { get; set; } = new();
    public List<RankingViewModel> TallesMasVendidos { get; set; } = new();
    public List<RankingViewModel> VentasPorCategoria { get; set; } = new();
    public List<StockTalle> StockBajo { get; set; } = new();
    public List<Pedido> UltimosPedidos { get; set; } = new();

    public int Conteo(string estado) => PedidosPorEstado.TryGetValue(estado, out var cantidad) ? cantidad : 0;
}

/// <summary>Una fila de un ranking: etiqueta, cantidad, monto y el porcentaje respecto del primero (para la barra).</summary>
public class RankingViewModel
{
    public string Etiqueta { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
    public double Porcentaje { get; set; }
}
