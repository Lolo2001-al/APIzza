namespace APIzza.Api.ClasesPizzas;

/// <summary>
/// Representa una pizza del menu.
/// </summary>
public class Pizza
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    /// <summary>
    /// clasica, especial o vegetariana.
    /// </summary>
    public string Categoria { get; set; } = "clasica";

    public string? Imagen { get; set; }

    public bool Disponible { get; set; } = true;

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
