using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Usuarios;
using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public class UsuarioException : Exception
{
    public UsuarioException(string mensaje) : base(mensaje) { }
}

public interface IUsuarioService
{
    Task<List<UsuarioListItemDto>> ListarStaffAsync();
    Task ActualizarEstadoAsync(int usuarioId, int adminIdActual, ActualizarEstadoUsuarioRequestDto request);
}

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _db;

    public UsuarioService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Solo administrador/operador — los conductores no se gestionan desde este panel.</summary>
    public async Task<List<UsuarioListItemDto>> ListarStaffAsync()
    {
        return await _db.Usuarios
            .Where(u => u.Rol == RolUsuario.administrador || u.Rol == RolUsuario.operador)
            .OrderBy(u => u.Rol).ThenBy(u => u.Nombre)
            .Select(u => new UsuarioListItemDto
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Correo = u.Correo,
                Cedula = u.Cedula,
                Rol = u.Rol.ToString(),
                Activo = u.Activo,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task ActualizarEstadoAsync(int usuarioId, int adminIdActual, ActualizarEstadoUsuarioRequestDto request)
    {
        if (usuarioId == adminIdActual && !request.Activo)
            throw new UsuarioException("No puedes desactivar tu propia cuenta.");

        var usuario = await _db.Usuarios.FindAsync(usuarioId)
            ?? throw new UsuarioException("Usuario no encontrado.");

        usuario.Activo = request.Activo;
        usuario.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
