namespace Pizzeria.Api.Clases;

/// <summary>Un pedido registrado en el sistema.</summary>
public class Pedido
{
    public int Id { get; init; }
    public Cliente Cliente { get; init; } = null!;
    public List<Linea> Lineas { get; init; } = new();
    public decimal Total => Lineas.Sum(l => l.Cantidad * l.PrecioUnitario);
    public EstadoPedido Estado { get; set; } = EstadoPedido.EsperaConfirmacion;
    public string? Detalle { get; set; }
    public DateTime Creado { get; init; } = DateTime.Now;
}
