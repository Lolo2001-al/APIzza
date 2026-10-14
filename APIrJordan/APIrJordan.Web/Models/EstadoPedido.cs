namespace APIrJordan.Web.Models;

/// <summary>
/// Máquina de estados de un pedido:
/// pendiente → pagado → enviado → entregado, y se puede cancelar mientras esté pendiente o pagado.
/// </summary>
public static class EstadoPedido
{
    public const string Pendiente = "pendiente";
    public const string Pagado = "pagado";
    public const string Enviado = "enviado";
    public const string Entregado = "entregado";
    public const string Cancelado = "cancelado";

    /// <summary>Circuito normal, en orden (sirve para dibujar la línea de tiempo).</summary>
    public static readonly string[] Circuito = { Pendiente, Pagado, Enviado, Entregado };

    public static readonly string[] Todos = { Pendiente, Pagado, Enviado, Entregado, Cancelado };

    public static bool EsValido(string? estado) => estado != null && Todos.Contains(estado);

    /// <summary>Desde un estado, a cuáles se puede pasar.</summary>
    public static IReadOnlyList<string> Siguientes(string estado) => estado switch
    {
        Pendiente => new[] { Pagado, Cancelado },
        Pagado => new[] { Enviado, Cancelado },
        Enviado => new[] { Entregado },
        _ => Array.Empty<string>(),
    };

    public static bool PuedePasarA(string actual, string nuevo) => Siguientes(actual).Contains(nuevo);

    /// <summary>Un pedido "activo" todavía no terminó su circuito.</summary>
    public static bool EsActivo(string estado) => estado is Pendiente or Pagado or Enviado;

    /// <summary>Posición dentro del circuito normal (-1 si está cancelado).</summary>
    public static int Posicion(string estado) => Array.IndexOf(Circuito, estado);

    public static string Nombre(string estado) => estado switch
    {
        Pendiente => "Pendiente",
        Pagado => "Pagado",
        Enviado => "Enviado",
        Entregado => "Entregado",
        Cancelado => "Cancelado",
        _ => estado,
    };

    /// <summary>Texto del botón que lleva a ese estado.</summary>
    public static string Accion(string estado) => estado switch
    {
        Pagado => "Marcar como pagado",
        Enviado => "Marcar como enviado",
        Entregado => "Marcar como entregado",
        Cancelado => "Cancelar pedido",
        _ => Nombre(estado),
    };

    public static string Descripcion(string estado) => estado switch
    {
        Pendiente => "Recibimos el pedido y esperamos el pago.",
        Pagado => "Pago acreditado. Preparamos el envío.",
        Enviado => "El pedido está en camino.",
        Entregado => "El cliente ya tiene sus zapatillas.",
        Cancelado => "El pedido se canceló y el stock volvió al catálogo.",
        _ => string.Empty,
    };
}
