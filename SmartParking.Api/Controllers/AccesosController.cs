using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Dtos.Reservas;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "operador,administrador")]
public class AccesosController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public AccesosController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    private int OperadorIdActual =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>El operador escanea el QR al momento de la entrada del vehículo.</summary>
    [HttpPost("validar-entrada")]
    public async Task<IActionResult> ValidarEntrada([FromBody] ValidarAccesoRequestDto request)
    {
        var resultado = await _reservaService.ValidarEntradaAsync(OperadorIdActual, request.QrToken);

        if (!resultado.Valido)
            return BadRequest(resultado);

        return Ok(resultado);
    }

    /// <summary>El operador escanea el mismo QR (o lo busca por placa) al momento de la salida.</summary>
    [HttpPost("registrar-salida")]
    public async Task<IActionResult> RegistrarSalida(
        [FromBody] ValidarAccesoRequestDto request,
        [FromQuery] string metodoPago = "efectivo")
    {
        try
        {
            var resultado = await _reservaService.RegistrarSalidaAsync(OperadorIdActual, request.QrToken, metodoPago);
            return Ok(resultado);
        }
        catch (ReservaException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
