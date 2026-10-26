using System.ComponentModel.DataAnnotations;

namespace APIzza.Web.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;
}
