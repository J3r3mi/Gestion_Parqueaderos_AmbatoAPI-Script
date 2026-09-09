using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using SmartParking.Api.Data;
using SmartParking.Api.Dtos.Auth;
using SmartParking.Api.Models;

namespace SmartParking.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var identificador = request.Identificador.Trim().ToLower();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u =>
            u.Activo &&
            (u.Correo.ToLower() == identificador || u.Cedula == identificador));

        if (usuario is null)
            return null;

        // Verificación real del hash BCrypt (nada de "password.length < 4" como en el mockup)
        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash);
        if (!passwordValida)
            return null;

        var (token, expiraEn) = _jwt.GenerarToken(usuario);

        return new LoginResponseDto
        {
            Token = token,
            ExpiraEn = expiraEn,
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Cedula = usuario.Cedula,
                Rol = usuario.Rol.ToString()
            }
        };
    }

    public async Task<(bool exito, string? error)> RegistrarConductorAsync(RegisterRequestDto request)
    {
        bool yaExiste = await _db.Usuarios.AnyAsync(u =>
            u.Correo.ToLower() == request.Correo.ToLower() || u.Cedula == request.Cedula);

        if (yaExiste)
            return (false, "Ya existe un usuario con ese correo o cédula.");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = RolUsuario.conductor,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool exito, string? error)> RegistrarStaffAsync(RegistrarStaffRequestDto request)
    {
        // Esta ruta la protege [Authorize(Roles = "administrador")] en el controller, pero
        // reforzamos aquí también: nunca se debe poder crear un 'conductor' por esta vía —
        // para eso ya existe el registro público (/api/auth/registro).
        if (request.Rol == RolUsuario.conductor)
            return (false, "Usa el registro público para crear cuentas de conductor.");

        bool yaExiste = await _db.Usuarios.AnyAsync(u =>
            u.Correo.ToLower() == request.Correo.ToLower() || u.Cedula == request.Cedula);

        if (yaExiste)
            return (false, "Ya existe un usuario con ese correo o cédula.");

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Cedula = request.Cedula,
            Correo = request.Correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Rol = request.Rol,
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return (true, null);
    }

    // -----------------------------------------------------------------
    // RECUPERACIÓN DE CONTRASEÑA
    // -----------------------------------------------------------------
    public async Task<SolicitarRecuperacionResponseDto> SolicitarRecuperacionAsync(SolicitarRecuperacionRequestDto request)
    {
        var identificador = request.Identificador.Trim().ToLower();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u =>
            u.Activo && (u.Correo.ToLower() == identificador || u.Cedula == identificador));

        // Respuesta genérica aunque el usuario no exista — evita que alguien pueda usar
        // este endpoint para averiguar qué correos/cédulas están registrados (enumeración de usuarios).
        const string mensajeGenerico =
            "Si el correo/cédula existe en el sistema, se enviaron instrucciones de recuperación.";

        if (usuario is null)
            return new SolicitarRecuperacionResponseDto { Mensaje = mensajeGenerico };

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        _db.PasswordResetTokens.Add(new Models.PasswordResetToken
        {
            UsuarioId = usuario.Id,
            Token = token,
            ExpiraEn = DateTime.UtcNow.AddMinutes(15), // ventana corta, estándar de la industria
            Usado = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // ---------------------------------------------------------------
        // AQUÍ IRÍA EL ENVÍO DE CORREO REAL EN PRODUCCIÓN, por ejemplo:
        //   await _emailSender.EnviarAsync(usuario.Correo, "Recupera tu contraseña",
        //       $"Usa este enlace: https://tuapp.com/restablecer?token={token}");
        // Este entorno no tiene un proveedor de correo configurado, así que el token
        // se devuelve directamente en la respuesta — SOLO para que puedas probar el flujo.
        // ---------------------------------------------------------------

        return new SolicitarRecuperacionResponseDto
        {
            Mensaje = mensajeGenerico,
            TokenSoloParaDemo = token
        };
    }

    public async Task<(bool exito, string? error)> RestablecerPasswordAsync(RestablecerPasswordRequestDto request)
    {
        var resetToken = await _db.PasswordResetTokens
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (resetToken is null)
            return (false, "Token inválido.");

        if (resetToken.Usado)
            return (false, "Este token ya fue utilizado.");

        if (resetToken.ExpiraEn < DateTime.UtcNow)
            return (false, "Este token expiró. Solicita uno nuevo.");

        resetToken.Usuario!.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
        resetToken.Usuario.UpdatedAt = DateTime.UtcNow;
        resetToken.Usado = true;

        await _db.SaveChangesAsync();
        return (true, null);
    }
}
