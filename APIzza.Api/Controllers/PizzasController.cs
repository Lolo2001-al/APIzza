using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIzza.Api.Biblioteca;
using APIzza.Api.ClasesPizzas;
using APIzza.Api.DTOs;

namespace APIzza.Api.Controllers;

/// <summary>
/// Endpoints para consultar y administrar el menu de pizzas.
/// </summary>
[ApiController]
[Route("api/pizzas")]
public class PizzasController : ControllerBase
{
    private readonly ApizzaDbContext _contexto;

    public PizzasController(ApizzaDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <summary>
    /// Lista todas las pizzas disponibles. Se puede filtrar por categoria.
    /// </summary>
    /// <param name="categoria">clasica, especial o vegetariana</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Pizza>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Pizza>>> ListarPizzas([FromQuery] string? categoria)
    {
        var query = _contexto.Pizzas.Where(p => p.Disponible);

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(p => p.Categoria == categoria);
        }

        return Ok(await query.ToListAsync());
    }

    /// <summary>
    /// Obtiene una pizza por su id.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Pizza), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pizza>> ObtenerPizza(int id)
    {
        var pizza = await _contexto.Pizzas.FindAsync(id);
        if (pizza == null)
        {
            return NotFound(new { error = "Pizza no encontrada" });
        }

        return Ok(pizza);
    }

    /// <summary>
    /// Crea una nueva pizza en el menu.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Pizza), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Pizza>> CrearPizza([FromBody] PizzaInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre) || input.Precio <= 0)
        {
            return BadRequest(new { error = "nombre y precio son obligatorios" });
        }

        var pizza = new Pizza
        {
            Nombre = input.Nombre,
            Descripcion = input.Descripcion,
            Precio = input.Precio,
            Categoria = input.Categoria,
            Imagen = input.Imagen,
            Disponible = input.Disponible,
        };

        _contexto.Pizzas.Add(pizza);
        await _contexto.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerPizza), new { id = pizza.Id }, pizza);
    }

    /// <summary>
    /// Actualiza una pizza existente.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Pizza), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pizza>> ActualizarPizza(int id, [FromBody] PizzaInputDto input)
    {
        var pizza = await _contexto.Pizzas.FindAsync(id);
        if (pizza == null)
        {
            return NotFound(new { error = "Pizza no encontrada" });
        }

        pizza.Nombre = input.Nombre;
        pizza.Descripcion = input.Descripcion;
        pizza.Precio = input.Precio;
        pizza.Categoria = input.Categoria;
        pizza.Imagen = input.Imagen;
        pizza.Disponible = input.Disponible;

        await _contexto.SaveChangesAsync();

        return Ok(pizza);
    }

    /// <summary>
    /// Elimina una pizza del menu.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EliminarPizza(int id)
    {
        var pizza = await _contexto.Pizzas.FindAsync(id);
        if (pizza == null)
        {
            return NotFound(new { error = "Pizza no encontrada" });
        }

        _contexto.Pizzas.Remove(pizza);
        await _contexto.SaveChangesAsync();

        return NoContent();
    }
}
