using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>Ficha de un modelo + otros modelos de la misma categoría.</summary>
public class ZapatillaDetalleViewModel
{
    public Zapatilla Zapatilla { get; set; } = new();
    public List<Zapatilla> Relacionadas { get; set; } = new();

    /// <summary>Talle preseleccionado al volver de un error (por ejemplo, sin stock suficiente).</summary>
    public decimal? TalleSeleccionado { get; set; }

    public bool SeVendio { get; set; }
}
