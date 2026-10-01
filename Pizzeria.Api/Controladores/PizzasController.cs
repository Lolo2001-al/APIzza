using Microsoft.AspNetCore.Mvc;
using Pizzeria.Api.Clases;

namespace Pizzeria.Api.Controladores;

/// <summary>Expone la carta de pizzas.</summary>
[ApiController]
[Route("api/pizzas")]
public class PizzasController : ControllerBase
{
    private readonly RepositorioPizzas _repositorio;

    public PizzasController(RepositorioPizzas repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Lista las pizzas disponibles.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Pizza>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Pizza>>> ObtenerPizzas()
    {
        var pizzas = await _repositorio.ObtenerTodasAsync();
        return Ok(pizzas);
    }
}
