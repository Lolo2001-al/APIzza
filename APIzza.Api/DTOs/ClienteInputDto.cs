namespace APIzza.Api.DTOs;

/// <summary>
/// Datos del cliente que llegan dentro del body al crear un pedido.
/// </summary>
public class ClienteInputDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
