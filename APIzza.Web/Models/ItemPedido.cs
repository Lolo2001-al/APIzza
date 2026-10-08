namespace APIzza.Web.Models;

/// <summary>Una linea de un pedido: una pizza y su cantidad.</summary>
public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    public int PizzaId { get; set; }
    public Pizza? Pizza { get; set; }

    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
}
