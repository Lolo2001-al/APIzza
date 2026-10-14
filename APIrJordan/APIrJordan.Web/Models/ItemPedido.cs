namespace APIrJordan.Web.Models;

/// <summary>Una línea de un pedido: un modelo, en un talle, con una cantidad y el precio del momento.</summary>
public class ItemPedido
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int ZapatillaId { get; set; }
    public Zapatilla? Zapatilla { get; set; }

    /// <summary>Se guarda en el ítem (y no solo en el stock) porque es parte del historial de la venta.</summary>
    public decimal Talle { get; set; }

    public int Cantidad { get; set; } = 1;

    /// <summary>Precio al momento de la compra. Si el modelo cambia de precio, el pedido viejo no cambia.</summary>
    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal => Cantidad * PrecioUnitario;

    public string TalleTexto => Talles.Formatear(Talle);
}
