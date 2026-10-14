namespace APIrJordan.Web.Models;

/// <summary>Un modelo de zapatilla del catálogo (por ejemplo, Air Jordan 1 Retro High OG "Chicago").</summary>
public class Zapatilla
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Combinación de colores con la que se conoce al modelo: "Chicago", "Bred", "Concord"...</summary>
    public string? Colorway { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }

    /// <summary>retro, lifestyle o basquet. Ver <see cref="Categorias"/>.</summary>
    public string Categoria { get; set; } = Categorias.Retro;

    /// <summary>Nombre del archivo dentro de wwwroot/images/zapatillas. Si es null se usa la ilustración genérica.</summary>
    public string? Imagen { get; set; }

    /// <summary>Si se muestra en la portada.</summary>
    public bool Destacada { get; set; }

    /// <summary>false = publicación pausada: no aparece en el catálogo ni se puede comprar, pero conserva su historial.</summary>
    public bool Activa { get; set; } = true;

    public DateTime CreadoEn { get; set; } = DateTime.Now;

    /// <summary>Stock por talle. Un modelo tiene una fila por cada talle (aunque la cantidad sea 0).</summary>
    public List<StockTalle> Talles { get; set; } = new();

    // ----- Propiedades calculadas (no se guardan en la base porque no tienen "set") -----

    public int StockTotal => Talles.Sum(t => t.Cantidad);

    public bool Agotada => Talles.Count > 0 && StockTotal == 0;

    public int TallesDisponibles => Talles.Count(t => t.Cantidad > 0);

    public string NombreCompleto => string.IsNullOrWhiteSpace(Colorway) ? Nombre : $"{Nombre} \"{Colorway}\"";
}
