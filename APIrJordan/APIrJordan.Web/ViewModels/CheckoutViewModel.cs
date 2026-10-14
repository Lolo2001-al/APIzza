using System.ComponentModel.DataAnnotations;

namespace APIrJordan.Web.ViewModels;

/// <summary>
/// Datos del comprador para confirmar la compra. Las líneas NO se postean:
/// se vuelven a leer del carrito (sesión) y de la base en el controlador.
/// </summary>
public class CheckoutViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre.")]
    [StringLength(120)]
    [Display(Name = "Nombre y apellido")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu email.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(160)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Ingresá la dirección de entrega.")]
    [StringLength(250)]
    [Display(Name = "Dirección de entrega")]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    [Display(Name = "Nota para la entrega (opcional)")]
    public string? Nota { get; set; }

    // Solo para mostrar el resumen
    public List<LineaCarritoViewModel> Lineas { get; set; } = new();
    public decimal Total { get; set; }
    public int Pares => Lineas.Sum(l => l.Cantidad);
}
