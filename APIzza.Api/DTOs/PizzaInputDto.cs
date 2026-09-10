namespace APIzza.Api.DTOs;

/// <summary>
/// Datos que llegan en el body al crear o editar una pizza.
/// </summary>
public class PizzaInputDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string Categoria { get; set; } = "clasica";
    public string? Imagen { get; set; }
    public bool Disponible { get; set; } = true;
}
