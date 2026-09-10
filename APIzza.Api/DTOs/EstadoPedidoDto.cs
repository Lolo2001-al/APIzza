namespace APIzza.Api.DTOs;

/// <summary>
/// Body para el endpoint que cambia el estado de un pedido.
/// </summary>
public class EstadoPedidoDto
{
    public string Estado { get; set; } = string.Empty;
}
