namespace Pizzeria.Api.Clases;

/// <summary>Lo que manda el cliente: qué pizza y cuántas.</summary>
public record ItemPedido(int PizzaId, int Cantidad);
