namespace APIzza.Api.DTOs;

/// <summary>
/// Body completo que se envia al crear un pedido: datos del cliente + items.
/// </summary>
public class PedidoInputDto
{
    public ClienteInputDto? Cliente { get; set; }
    public List<ItemPedidoDto> Items { get; set; } = new();
}
