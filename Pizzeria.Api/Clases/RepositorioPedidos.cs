using System.Collections.Concurrent;

namespace Pizzeria.Api.Clases;

/// <summary>Guarda los pedidos en memoria. Expone todo como Task para que el controlador
/// siempre haga await, igual que haría con una base de datos de verdad.</summary>
public class RepositorioPedidos
{
    private readonly ConcurrentDictionary<int, Pedido> _pedidos = new();
    private int _contador = 0;

    public int SiguienteId() => Interlocked.Increment(ref _contador);

    public Task AgregarAsync(Pedido pedido)
    {
        _pedidos[pedido.Id] = pedido;
        return Task.CompletedTask;
    }

    public Task<List<Pedido>> ObtenerTodosAsync() =>
        Task.FromResult(_pedidos.Values.OrderByDescending(p => p.Id).ToList());

    public Task<Pedido?> ObtenerPorIdAsync(int id)
    {
        _pedidos.TryGetValue(id, out var pedido);
        return Task.FromResult(pedido);
    }
}
