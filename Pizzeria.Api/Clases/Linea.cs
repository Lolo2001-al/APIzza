namespace Pizzeria.Api.Clases;

/// <summary>Una línea del pedido ya confirmada, con el precio de ese momento.</summary>
public record Linea(int PizzaId, string Nombre, int Cantidad, decimal PrecioUnitario);
