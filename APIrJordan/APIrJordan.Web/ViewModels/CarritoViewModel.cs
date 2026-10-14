using APIrJordan.Web.Models;

namespace APIrJordan.Web.ViewModels;

/// <summary>El carrito "resuelto" contra la base: cada línea con su modelo, precio y stock actual.</summary>
public class CarritoViewModel
{
    public List<LineaCarritoViewModel> Lineas { get; set; } = new();

    public int Pares => Lineas.Sum(l => l.Cantidad);

    public decimal Total => Lineas.Sum(l => l.Subtotal);

    public bool EstaVacio => Lineas.Count == 0;

    /// <summary>Alguna línea no se puede comprar tal como está (sin stock, modelo pausado...).</summary>
    public bool TieneProblemas => Lineas.Any(l => l.Problema != null);
}

public class LineaCarritoViewModel
{
    public Zapatilla Zapatilla { get; set; } = new();
    public decimal Talle { get; set; }
    public int Cantidad { get; set; }
    public int StockDisponible { get; set; }
    public string? Problema { get; set; }

    public decimal Subtotal => Zapatilla.Precio * Cantidad;

    public string TalleTexto => Talles.Formatear(Talle);
}
