using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Dtos.Parqueaderos;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // cualquier usuario autenticado puede LEER (conductor necesita esto para el mapa)
public class ParqueaderosController : ControllerBase
{
    private readonly IParqueaderoService _service;

    public ParqueaderosController(IParqueaderoService service)
    {
        _service = service;
    }

    private int UsuarioIdActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---------------- LECTURA (todos los roles autenticados) ----------------

    /// <summary>Lista de parqueaderos activos con cupos libres — alimenta los marcadores del mapa.</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var parqueaderos = await _service.ListarActivosAsync();
        return Ok(parqueaderos);
    }

    /// <summary>Plazas de un parqueadero (el conductor elige cuál reservar).</summary>
    [HttpGet("{id:int}/plazas")]
    public async Task<IActionResult> ListarPlazas(int id)
    {
        var plazas = await _service.ListarPlazasAsync(id);
        return Ok(plazas);
    }

    /// <summary>Tarifa vigente por tipo de vehículo — para mostrar el precio antes de reservar.</summary>
    [HttpGet("{id:int}/tarifas")]
    public async Task<IActionResult> ListarTarifas(int id)
    {
        var tarifas = await _service.ListarTarifasVigentesAsync(id);
        return Ok(tarifas);
    }

    // ---------------- ESCRITURA (solo Administrador) ----------------

    [HttpPost]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearParqueaderoRequestDto request)
    {
        var id = await _service.CrearParqueaderoAsync(UsuarioIdActual, request);
        return CreatedAtAction(nameof(Listar), new { }, new { id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarParqueaderoRequestDto request)
    {
        try
        {
            await _service.ActualizarParqueaderoAsync(id, request);
            return Ok(new { mensaje = "Parqueadero actualizado." });
        }
        catch (ParqueaderoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/plazas")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> AgregarPlaza(int id, [FromBody] CrearPlazaRequestDto request)
    {
        try
        {
            var plazaId = await _service.AgregarPlazaAsync(id, request);
            return CreatedAtAction(nameof(ListarPlazas), new { id }, new { plazaId });
        }
        catch (ParqueaderoException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}/plazas/{plazaId:int}/estado")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> ActualizarEstadoPlaza(
        int id, int plazaId, [FromBody] ActualizarEstadoPlazaRequestDto request)
    {
        try
        {
            await _service.ActualizarEstadoPlazaAsync(id, plazaId, request);
            return Ok(new { mensaje = "Estado de plaza actualizado." });
        }
        catch (ParqueaderoException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/tarifas")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> AgregarTarifa(int id, [FromBody] CrearTarifaRequestDto request)
    {
        try
        {
            await _service.AgregarTarifaAsync(id, request);
            return Ok(new { mensaje = "Tarifa creada correctamente." });
        }
        catch (ParqueaderoException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}
