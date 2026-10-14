namespace APIrJordan.Web.Models;

/// <summary>Cuántos pares hay de un talle de un modelo. Es la tabla que diferencia una zapatillería de una pizzería.</summary>
public class StockTalle
{
    public int Id { get; set; }

    public int ZapatillaId { get; set; }
    public Zapatilla? Zapatilla { get; set; }

    /// <summary>Talle argentino/europeo: 38, 40.5, 42...</summary>
    public decimal Talle { get; set; }

    /// <summary>
    /// Pares disponibles. Está configurado como token de concurrencia: si dos personas compran el último par
    /// al mismo tiempo, la segunda operación falla en vez de dejar el stock en negativo.
    /// </summary>
    public int Cantidad { get; set; }

    public string TalleTexto => Talles.Formatear(Talle);

    public bool Disponible => Cantidad > 0;

    /// <summary>Quedan 1 o 2 pares: se avisa en la ficha.</summary>
    public bool UltimosPares => Cantidad > 0 && Cantidad <= 2;
}
