using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartParking.Api.Dtos.Auth;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login para Conductor, Administrador u Operador.
    /// Acepta correo o cédula como identificador.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _authService.LoginAsync(request);

        if (resultado is null)
            return Unauthorized(new { mensaje = "Credenciales inválidas." });

        return Ok(resultado);
    }

    /// <summary>
    /// Registro público de un Conductor (Administrador y Operador se crean desde el panel admin).
    /// </summary>
    [HttpPost("registro")]
    public async Task<IActionResult> Registro([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, error) = await _authService.RegistrarConductorAsync(request);

        if (!exito)
            return Conflict(new { mensaje = error });

        return Ok(new { mensaje = "Usuario registrado correctamente. Ya puede iniciar sesión." });
    }

    /// <summary>
    /// Registro de personal (Administrador u Operador). Solo un Administrador ya autenticado
    /// puede crear estas cuentas — a diferencia de /registro, esta ruta NO es pública.
    /// </summary>
    [HttpPost("registrar-staff")]
    [Authorize(Roles = "administrador")]
    public async Task<IActionResult> RegistrarStaff([FromBody] RegistrarStaffRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, error) = await _authService.RegistrarStaffAsync(request);

        if (!exito)
            return Conflict(new { mensaje = error });

        return Ok(new { mensaje = $"Usuario ({request.Rol}) registrado correctamente." });
    }

    /// <summary>
    /// Solicita recuperación de contraseña. Siempre responde con el mismo mensaje genérico,
    /// exista o no el usuario, para no permitir enumerar cuentas registradas.
    /// </summary>
    [HttpPost("solicitar-recuperacion")]
    [EnableRateLimiting("recuperacion-password")]
    public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _authService.SolicitarRecuperacionAsync(request);
        return Ok(resultado);
    }

    /// <summary>Confirma la recuperación: cambia la contraseña usando el token recibido.</summary>
    [HttpPost("restablecer-password")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (exito, error) = await _authService.RestablecerPasswordAsync(request);

        if (!exito)
            return BadRequest(new { mensaje = error });

        return Ok(new { mensaje = "Contraseña actualizada correctamente. Ya puedes iniciar sesión." });
    }
}
