namespace APIrJordan.Web.Models;

/// <summary>Cabecera de una compra. Los pares comprados están en <see cref="Items"/>.</summary>
public class Pedido
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>pendiente, pagado, enviado, entregado o cancelado. Ver <see cref="EstadoPedido"/>.</summary>
    public string Estado { get; set; } = EstadoPedido.Pendiente;

    public decimal Total { get; set; }

    /// <summary>Comentario opcional del cliente para la entrega.</summary>
    public string? Nota { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.Now;

    /// <summary>Último cambio de estado.</summary>
    public DateTime ActualizadoEn { get; set; } = DateTime.Now;

    public List<ItemPedido> Items { get; set; } = new();

    public int CantidadPares => Items.Sum(i => i.Cantidad);

    public bool Cancelado => Estado == EstadoPedido.Cancelado;
}
