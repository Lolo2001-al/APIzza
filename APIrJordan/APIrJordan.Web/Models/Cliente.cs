namespace APIrJordan.Web.Models;

/// <summary>Persona que hizo uno o más pedidos. Se identifica por el email.</summary>
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public DateTime CreadoEn { get; set; } = DateTime.Now;

    public List<Pedido> Pedidos { get; set; } = new();
}
