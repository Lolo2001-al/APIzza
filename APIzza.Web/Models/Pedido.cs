namespace APIzza.Web.Models;

/// <summary>Pedido de un cliente, compuesto por uno o mas items.</summary>
public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public string Estado { get; set; } = "pendiente";
    public decimal Total { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.Now;

    public List<ItemPedido> Items { get; set; } = new();
}
