using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Dtos.Reservas;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // requiere JWT válido; el rol específico se revisa por acción
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    private int UsuarioIdActual =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Authorize(Roles = "conductor,administrador")]
    public async Task<IActionResult> Crear([FromBody] CrearReservaRequestDto request)
    {
        try
        {
            var reserva = await _reservaService.CrearReservaAsync(UsuarioIdActual, request);
            return CreatedAtAction(nameof(MisReservas), new { }, reserva);
        }
        catch (ReservaException ex)
        {
            // 409 Conflict: es precisamente el caso de "otro conductor reservó primero"
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = "conductor,administrador")]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            await _reservaService.CancelarReservaAsync(UsuarioIdActual, id);
            return Ok(new { mensaje = "Reserva cancelada correctamente." });
        }
        catch (ReservaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("mias")]
    [Authorize(Roles = "conductor,administrador")]
    public async Task<IActionResult> MisReservas()
    {
        var reservas = await _reservaService.MisReservasAsync(UsuarioIdActual);
        return Ok(reservas);
    }
}
