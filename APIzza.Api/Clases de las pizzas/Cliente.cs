namespace APIzza.Api.ClasesPizzas;

/// <summary>
/// Representa a un cliente que hizo uno o mas pedidos.
/// </summary>
public class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public List<Pedido> Pedidos { get; set; } = new();
}
