using System.ComponentModel.DataAnnotations;

namespace APIzza.Web.ViewModels;

public class PizzaCrearViewModel
{
    [Required(ErrorMessage = "Ingresá el nombre de la pizza.")]
    [StringLength(120)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "Ingresá un precio válido.")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Required]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = "clasica";
}
