using System.ComponentModel.DataAnnotations;

namespace APIzza.Web.ViewModels;

/// <summary>
/// Modelo del formulario "Hacer pedido". Va del controlador a la vista (GET)
/// y de la vista al controlador (POST, por model binding).
/// </summary>
public class PedidoViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu email")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Ingresá la dirección de entrega")]
    [Display(Name = "Dirección de entrega")]
    public string Direccion { get; set; } = string.Empty;

    public List<LineaPedidoViewModel> Lineas { get; set; } = new();
}

/// <summary>Una pizza del formulario con la cantidad que elige el cliente.</summary>
public class LineaPedidoViewModel
{
    public int PizzaId { get; set; }

    // Solo para mostrar (no se confía en lo que vuelve del navegador).
    // Son "string?" a propósito: así MVC no las exige como obligatorias en el POST.
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }

    [Range(0, 20, ErrorMessage = "Entre 0 y 20")]
    public int Cantidad { get; set; }
}
