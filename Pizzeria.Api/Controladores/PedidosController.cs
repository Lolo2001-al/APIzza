using Microsoft.AspNetCore.Mvc;
using Pizzeria.Api.Clases;

namespace Pizzeria.Api.Controladores;

/// <summary>Recibe pedidos con una o más pizzas y expone su estado.</summary>
[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly RepositorioPizzas _pizzas;
    private readonly RepositorioPedidos _pedidos;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(RepositorioPizzas pizzas, RepositorioPedidos pedidos, ILogger<PedidosController> logger)
    {
        _pizzas = pizzas;
        _pedidos = pedidos;
        _logger = logger;
    }

    /// <summary>Crea un pedido con una o más pizzas y lo manda a la cocina.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pedido>> CrearPedido([FromBody] NuevoPedido nuevoPedido)
    {
        if (nuevoPedido?.Cliente is null || string.IsNullOrWhiteSpace(nuevoPedido.Cliente.Nombre)
            || string.IsNullOrWhiteSpace(nuevoPedido.Cliente.Telefono) || string.IsNullOrWhiteSpace(nuevoPedido.Cliente.Direccion))
            return BadRequest(new { error = "Completá nombre, teléfono y dirección." });

        if (nuevoPedido.Items is null || nuevoPedido.Items.Count == 0)
            return BadRequest(new { error = "El pedido necesita al menos una pizza." });

        var lineas = new List<Linea>();
        foreach (var item in nuevoPedido.Items)
        {
            var pizza = await _pizzas.ObtenerPorIdAsync(item.PizzaId);
            if (pizza is null)
                return BadRequest(new { error = $"La pizza {item.PizzaId} no existe." });
            if (item.Cantidad is < 1 or > 20)
                return BadRequest(new { error = "La cantidad por pizza debe estar entre 1 y 20." });
            lineas.Add(new Linea(pizza.Id, pizza.Nombre, item.Cantidad, pizza.Precio));
        }

        var pedido = new Pedido { Id = _pedidos.SiguienteId(), Cliente = nuevoPedido.Cliente, Lineas = lineas };
        await _pedidos.AgregarAsync(pedido);
        _logger.LogInformation("Pedido #{Id} recibido de {Nombre}", pedido.Id, pedido.Cliente.Nombre);

        _ = Despachador.ProcesarAsync(pedido, _logger);   // sigue en 2do plano, no bloquea la respuesta
        return CreatedAtAction(nameof(ObtenerPorId), new { id = pedido.Id }, pedido);
    }

    /// <summary>Lista todos los pedidos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Pedido>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Pedido>>> ObtenerTodos()
    {
        var pedidos = await _pedidos.ObtenerTodosAsync();
        return Ok(pedidos);
    }

    /// <summary>Consulta el estado de un pedido puntual.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pedido>> ObtenerPorId(int id)
    {
        var pedido = await _pedidos.ObtenerPorIdAsync(id);
        if (pedido is null) return NotFound(new { error = $"No existe el pedido #{id}." });
        return Ok(pedido);
    }
}
