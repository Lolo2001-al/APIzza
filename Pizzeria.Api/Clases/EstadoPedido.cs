namespace Pizzeria.Api.Clases;

/// <summary>Los estados por los que pasa un pedido. Cancelado se usa cuando algo falla.</summary>
public enum EstadoPedido { EsperaConfirmacion, EnPreparacion, EnViaje, Entregado, Cancelado }
