namespace Pizzeria.Api.Clases;

/// <summary>Cuerpo del POST /api/pedidos: el cliente y una lista de pizzas.</summary>
public record NuevoPedido(Cliente Cliente, List<ItemPedido> Items);
