using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Services.Routing;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RutasController : ControllerBase
{
    private readonly IRoutingService _routingService;

    public RutasController(IRoutingService routingService)
    {
        _routingService = routingService;
    }

    /// <summary>
    /// Calcula la ruta óptima desde la ubicación GPS del conductor hasta un parqueadero.
    /// `algoritmo` acepta "dijkstra" o "astar" (por defecto astar) — útil para comparar
    /// ambos enfoques en la demo/defensa, ya que la respuesta incluye `nodosExplorados`.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CalcularRuta(
        [FromQuery] double origenLat,
        [FromQuery] double origenLng,
        [FromQuery] int parqueaderoId,
        [FromQuery] string algoritmo = "astar")
    {
        try
        {
            var ruta = await _routingService.CalcularRutaAsync(origenLat, origenLng, parqueaderoId, algoritmo);
            return Ok(ruta);
        }
        catch (RutaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Calcula la ruta con AMBOS algoritmos en la misma llamada — pensado específicamente
    /// para el capítulo de la tesis que compara Dijkstra vs. A* (tiempo, distancia, nodos explorados).
    /// </summary>
    [HttpGet("comparar")]
    public async Task<IActionResult> CompararAlgoritmos(
        [FromQuery] double origenLat, [FromQuery] double origenLng, [FromQuery] int parqueaderoId)
    {
        try
        {
            var dijkstra = await _routingService.CalcularRutaAsync(origenLat, origenLng, parqueaderoId, "dijkstra");
            var astar = await _routingService.CalcularRutaAsync(origenLat, origenLng, parqueaderoId, "astar");
            return Ok(new { dijkstra, astar });
        }
        catch (RutaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
