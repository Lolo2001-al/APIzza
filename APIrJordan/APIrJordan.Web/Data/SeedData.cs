using APIrJordan.Web.Models;

namespace APIrJordan.Web.Data;

/// <summary>
/// Garantiza que la base exista y carga un catálogo de ejemplo solo si está vacío.
/// Es el mismo contenido que database/inserts.sql: si ya ejecutaste ese script, acá no se agrega nada.
/// </summary>
public static class SeedData
{
    public static void Inicializar(ApirJordanDbContext contexto)
    {
        contexto.Database.EnsureCreated();

        if (contexto.Zapatillas.Any())
        {
            return;
        }

        // ----- Catálogo -----
        var chicago = Crear("Air Jordan 1 Retro High OG", "Chicago", Categorias.Retro, 389999, "aj1-high-chicago.jpg",
            "La silueta que empezó todo, en la combinación más famosa: cuero blanco, rojo y negro sobre la horma original de 1985.",
            destacada: true, diasAtras: 40,
            (39, 2), (40, 3), (40.5m, 1), (41, 4), (42, 3), (42.5m, 2), (43, 2), (44, 1));

        var bred4 = Crear("Air Jordan 4 Retro", "Bred Reimagined", Categorias.Retro, 419999, "aj4-bred.jpg",
            "Nubuck negro, mallas laterales y los detalles en rojo del par original de 1989. Amortiguación Air visible en el talón.",
            destacada: true, diasAtras: 35,
            (40, 2), (41, 3), (42, 2), (43, 1), (44, 2));

        var concord = Crear("Air Jordan 11 Retro", "Concord", Categorias.Retro, 449999, "aj11-concord.jpg",
            "Charol negro sobre malla blanca y suela translúcida. La más elegante de la línea, pensada para la cancha.",
            destacada: true, diasAtras: 30,
            (41, 1), (42, 2), (43, 2), (45, 1));

        var cement = Crear("Air Jordan 3 Retro", "White Cement", Categorias.Retro, 399999, "aj3-white-cement.jpg",
            "Cuero blanco con el clásico estampado elefante en puntera y talón. La primera con Air visible.",
            destacada: false, diasAtras: 28,
            (40, 1), (41, 2), (42, 1), (44, 1));

        var bredToe = Crear("Air Jordan 1 Low", "Bred Toe", Categorias.Lifestyle, 249999, "aj1-low-bred-toe.jpg",
            "La versión baja de la 1, liviana y fácil de usar todos los días. Puntera roja, paneles negros y cuarto blanco.",
            destacada: true, diasAtras: 21,
            (38, 3), (39, 4), (40, 5), (41, 5), (42, 4), (43, 3), (44, 2), (45, 1));

        var spizike = Crear("Jordan Spizike Low", "White/Black", Categorias.Lifestyle, 299999, "spizike-low.jpg",
            "Un híbrido que mezcla detalles de la 3, la 4, la 5 y la 6 en una silueta baja. Cuero blanco con acentos negros.",
            destacada: false, diasAtras: 18,
            (40, 2), (42, 2), (43, 1));

        var maxAura = Crear("Jordan Max Aura 6", "Black/Red", Categorias.Lifestyle, 219999, "max-aura-6.jpg",
            "Inspirada en la Air Jordan 3 pero con unidad Max Air en el talón. Cómoda para caminar todo el día.",
            destacada: false, diasAtras: 14,
            (39, 2), (40, 3), (41, 3), (42, 4), (43, 2));

        var luka = Crear("Jordan Luka 3", "Motorsport", Categorias.Basquet, 259999, "luka-3.jpg",
            "Tejido reforzado, espuma Formula 23 y una base ancha para frenar y cambiar de dirección.",
            destacada: false, diasAtras: 10,
            (41, 3), (42, 4), (42.5m, 2), (43, 3), (44, 2), (45, 2));

        var tatum = Crear("Jordan Tatum 3", "Zen", Categorias.Basquet, 239999, "tatum-3.jpg",
            "Liviana y de perfil bajo. Malla técnica verde salvia y suela de goma natural.",
            destacada: false, diasAtras: 7,
            (40, 2), (41, 2), (42, 3), (43, 1));

        var taxi = Crear("Air Jordan 12 Retro", "Taxi", Categorias.Retro, 429999, "aj12-taxi.jpg",
            "Cuero blanco con costuras radiales y guardabarros negro. Edición agotada: avisamos cuando vuelva.",
            destacada: false, diasAtras: 3);   // sin stock en ningún talle → aparece como agotada

        contexto.Zapatillas.AddRange(chicago, bred4, concord, cement, bredToe, spizike, maxAura, luka, tatum, taxi);

        // ----- Clientes -----
        var valentina = new Cliente { Nombre = "Valentina Ríos", Email = "valentina.rios@example.com", Telefono = "11 5555 1234", Direccion = "Av. Corrientes 1234, CABA", CreadoEn = DateTime.Now.AddDays(-12) };
        var tomas = new Cliente { Nombre = "Tomás Herrera", Email = "tomas.herrera@example.com", Telefono = "11 4444 9876", Direccion = "Calle 12 n.º 456, La Plata", CreadoEn = DateTime.Now.AddDays(-5) };
        var lucia = new Cliente { Nombre = "Lucía Fernández", Email = "lucia.fernandez@example.com", Telefono = null, Direccion = "San Martín 789, Rosario", CreadoEn = DateTime.Now.AddDays(-1) };

        contexto.Clientes.AddRange(valentina, tomas, lucia);

        // ----- Pedidos de ejemplo (el stock de arriba ya tiene descontadas estas ventas) -----
        contexto.Pedidos.AddRange(
            CrearPedido(valentina, EstadoPedido.Entregado, diasAtras: 12, actualizadoHaceDias: 8, nota: "Dejar en portería si no hay nadie.",
                (chicago, 41, 1), (bredToe, 41, 1)),
            CrearPedido(tomas, EstadoPedido.Enviado, diasAtras: 5, actualizadoHaceDias: 2, nota: null,
                (luka, 43, 1)),
            CrearPedido(valentina, EstadoPedido.Cancelado, diasAtras: 3, actualizadoHaceDias: 2, nota: null,
                (concord, 42, 1)),
            CrearPedido(lucia, EstadoPedido.Pendiente, diasAtras: 1, actualizadoHaceDias: 1, nota: "Horario de entrega: después de las 18.",
                (bred4, 42, 1), (tatum, 42, 2)));

        contexto.SaveChanges();
    }

    /// <summary>Arma un modelo con una fila de stock por cada talle de la curva (0 si no se indica).</summary>
    private static Zapatilla Crear(string nombre, string colorway, string categoria, decimal precio, string imagen,
        string descripcion, bool destacada, int diasAtras, params (decimal Talle, int Cantidad)[] stock)
    {
        var zapatilla = new Zapatilla
        {
            Nombre = nombre,
            Colorway = colorway,
            Categoria = categoria,
            Precio = precio,
            Imagen = imagen,
            Descripcion = descripcion,
            Destacada = destacada,
            Activa = true,
            CreadoEn = DateTime.Now.AddDays(-diasAtras),
        };

        foreach (var talle in Talles.Todos)
        {
            var cantidad = stock.FirstOrDefault(s => s.Talle == talle).Cantidad;
            zapatilla.Talles.Add(new StockTalle { Talle = talle, Cantidad = cantidad });
        }

        return zapatilla;
    }

    private static Pedido CrearPedido(Cliente cliente, string estado, int diasAtras, int actualizadoHaceDias, string? nota,
        params (Zapatilla Zapatilla, decimal Talle, int Cantidad)[] items)
    {
        var pedido = new Pedido
        {
            Cliente = cliente,
            Estado = estado,
            Nota = nota,
            CreadoEn = DateTime.Now.AddDays(-diasAtras),
            ActualizadoEn = DateTime.Now.AddDays(-actualizadoHaceDias),
        };

        foreach (var item in items)
        {
            pedido.Items.Add(new ItemPedido
            {
                Zapatilla = item.Zapatilla,
                Talle = item.Talle,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.Zapatilla.Precio,
            });
        }

        pedido.Total = pedido.Items.Sum(i => i.Subtotal);
        return pedido;
    }
}
