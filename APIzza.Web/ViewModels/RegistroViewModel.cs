using System.ComponentModel.DataAnnotations;

namespace APIzza.Web.ViewModels;

public class RegistroViewModel
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required, Compare(nameof(Contrasena)), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
