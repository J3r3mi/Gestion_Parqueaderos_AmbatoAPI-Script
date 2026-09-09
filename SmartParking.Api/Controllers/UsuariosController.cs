using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Api.Dtos.Usuarios;
using SmartParking.Api.Services;

namespace SmartParking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "administrador")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    private int AdminIdActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lista administradores y operadores (no conductores).</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _usuarioService.ListarStaffAsync();
        return Ok(usuarios);
    }

    /// <summary>Activa o desactiva una cuenta de personal (soft-delete, no se borra el registro).</summary>
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoUsuarioRequestDto request)
    {
        try
        {
            await _usuarioService.ActualizarEstadoAsync(id, AdminIdActual, request);
            return Ok(new { mensaje = "Estado actualizado." });
        }
        catch (UsuarioException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
