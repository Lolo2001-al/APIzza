using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIzza.Api.Biblioteca;
using APIzza.Api.ClasesPizzas;
using APIzza.Api.DTOs;

namespace APIzza.Api.Controllers;

/// <summary>
/// Endpoints para crear y consultar pedidos de clientes.
/// </summary>
[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly ApizzaDbContext _contexto;

    private static readonly string[] EstadosValidos =
    {
        "pendiente", "en_preparacion", "en_camino", "entregado", "cancelado",
    };

    public PedidosController(ApizzaDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Crea un nuevo pedido con uno o mas items. Si el email del cliente ya existe,
    /// reutiliza ese cliente en vez de crear uno nuevo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pedido>> CrearPedido([FromBody] PedidoInputDto input)
    {
        if (input.Items == null || input.Items.Count == 0)
        {
            return BadRequest(new { error = "El pedido debe tener al menos un item" });
        }

        Cliente? cliente = null;
        if (input.Cliente != null && !string.IsNullOrWhiteSpace(input.Cliente.Email))
        {
            cliente = await _contexto.Clientes
                .FirstOrDefaultAsync(c => c.Email == input.Cliente.Email);

            if (cliente == null)
            {
                cliente = new Cliente
                {
                    Nombre = input.Cliente.Nombre,
                    Email = input.Cliente.Email,
                    Telefono = input.Cliente.Telefono,
                    Direccion = input.Cliente.Direccion,
                };
                _contexto.Clientes.Add(cliente);
                await _contexto.SaveChangesAsync();
            }
        }

        var pedido = new Pedido
        {
            ClienteId = cliente?.Id,
            Estado = "pendiente",
        };

        decimal total = 0;

        foreach (var item in input.Items)
        {
            var pizza = await _contexto.Pizzas.FindAsync(item.PizzaId);
            if (pizza == null)
            {
                return BadRequest(new { error = $"Pizza con id {item.PizzaId} no existe" });
            }

            var cantidad = item.Cantidad <= 0 ? 1 : item.Cantidad;
            total += pizza.Precio * cantidad;

            pedido.Items.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                Cantidad = cantidad,
                PrecioUnitario = pizza.Precio,
            });
        }

        pedido.Total = total;

        _contexto.Pedidos.Add(pedido);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPedido), new { id = pedido.Id }, pedido);
    }

    /// <summary>
    /// Lista todos los pedidos, del mas reciente al mas viejo.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Pedido>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Pedido>>> ListarPedidos()
    {
        var pedidos = await _contexto.Pedidos
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync();

        return Ok(pedidos);
    }

    /// <summary>
    /// Obtiene un pedido por id, incluyendo sus items y la pizza de cada uno.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pedido>> ObtenerPedido(int id)
    {
        var pedido = await _contexto.Pedidos
            .Include(p => p.Items)
            .ThenInclude(i => i.Pizza)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound(new { error = "Pedido no encontrado" });
        }

        return Ok(pedido);
    }

    /// <summary>
    /// Cambia el estado de un pedido (pendiente, en_preparacion, en_camino, entregado, cancelado).
    /// </summary>
    [HttpPatch("{id}/estado")]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pedido>> ActualizarEstado(int id, [FromBody] EstadoPedidoDto input)
    {
        if (!EstadosValidos.Contains(input.Estado))
        {
            return BadRequest(new { error = $"Estado invalido. Usar uno de: {string.Join(", ", EstadosValidos)}" });
        }

        var pedido = await _contexto.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound(new { error = "Pedido no encontrado" });
        }

        pedido.Estado = input.Estado;
        await _contexto.SaveChangesAsync();

        return Ok(pedido);
    }
}
