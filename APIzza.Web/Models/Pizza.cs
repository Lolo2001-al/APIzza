namespace APIzza.Web.Models;

/// <summary>Una pizza del menu.</summary>
public class Pizza
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }

    /// <summary>clasica, especial o vegetariana.</summary>
    public string Categoria { get; set; } = "clasica";
}
