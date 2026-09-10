namespace APIzza.Api.ClasesPizzas;

/// <summary>
/// Representa un pedido realizado por un cliente, compuesto por uno o mas items.
/// </summary>
public class Pedido
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    /// <summary>
    /// pendiente, en_preparacion, en_camino, entregado o cancelado.
    /// </summary>
    public string Estado { get; set; } = "pendiente";

    public decimal Total { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public List<ItemPedido> Items { get; set; } = new();
}
