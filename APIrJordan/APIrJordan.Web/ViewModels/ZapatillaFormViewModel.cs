using System.ComponentModel.DataAnnotations;
using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Formulario de alta y edición de un modelo, con su stock por talle.</summary>
public class ZapatillaFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre del modelo.")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres.")]
    [Display(Name = "Modelo")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "Máximo 80 caracteres.")]
    [Display(Name = "Colorway")]
    public string? Colorway { get; set; }

    [StringLength(600, ErrorMessage = "Máximo 600 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Range(1, 99999999, ErrorMessage = "Ingresá un precio válido (en pesos, sin decimales).")]
    [Display(Name = "Precio")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "Elegí una categoría.")]
    [Display(Name = "Categoría")]
    public string Categoria { get; set; } = Categorias.Retro;

    [Display(Name = "Ilustración")]
    public string? Imagen { get; set; }

    [Display(Name = "Mostrar en la portada")]
    public bool Destacada { get; set; }

    [Display(Name = "Publicada en el catálogo")]
    public bool Activa { get; set; } = true;

    /// <summary>Una fila por talle de la curva. Se postea como Stock[i].Talle / Stock[i].Cantidad.</summary>
    public List<StockTalleFormViewModel> Stock { get; set; } = new();

    public bool EsEdicion => Id.HasValue;

    public int StockTotal => Stock.Sum(s => s.Cantidad);
}

public class StockTalleFormViewModel
{
    public decimal Talle { get; set; }

    [Range(0, 999, ErrorMessage = "Entre 0 y 999.")]
    public int Cantidad { get; set; }

    public string TalleTexto => Talles.Formatear(Talle);
}
