namespace APIzza.Api.DTOs;

/// <summary>
/// Una linea del pedido: que pizza y en que cantidad.
/// </summary>
public class ItemPedidoDto
{
    public int PizzaId { get; set; }
    public int Cantidad { get; set; } = 1;
}
