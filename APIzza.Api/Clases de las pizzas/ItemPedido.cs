namespace APIzza.Api.ClasesPizzas;

/// <summary>
/// Una linea dentro de un pedido: una pizza puntual y su cantidad.
/// </summary>
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
